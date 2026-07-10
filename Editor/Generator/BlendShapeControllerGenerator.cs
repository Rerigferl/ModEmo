using nadena.dev.ndmf.util;
using Numeira.Animation;

namespace Numeira;

internal static class BlendShapeControllerGenerator
{
    public static void Generate(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        var modEmo = context.GetModEmoContext().Root;
        var data = context.GetData();

        var layer = animatorController.AddLayer("[ModEmo] BlendShape Control");
        var stateMachine = layer.StateMachine.WithDefaultWriteDefaults(true);
        var state = stateMachine.AddState("(WD On)");
        var blendTree = new DirectBlendTreeBuilder
        {
            Name = "BlendShapes",
            DefaultDirectBlendParameter = ParameterNames.Internal.One
        };
        state.Motion = blendTree;

        var facePath = data.Face.gameObject.AvatarRootPath();

        var bitBuffer = (stackalloc int[32]);

        foreach (var blendShape in data.FaceInfo.BlendShapes)
        {
            var name = blendShape.Name;
            var usageInfo = blendShape.UsageInfo;
            if (!usageInfo.AllowControl)
                continue;

            var min = new AnimationClipBuilder() { Name = $"{name} Min" };
            var max = new AnimationClipBuilder() { Name = $"{name} Max" };
            var @default = new AnimationClipBuilder() { Name = $"{name} Default" };
            var propertyName = $"blendShape.{name}";

            min.Add(new EditorCurveBinding() { path = facePath, propertyName = propertyName, type = typeof(SkinnedMeshRenderer) }, 0, 0);
            max.Add(new EditorCurveBinding() { path = facePath, propertyName = propertyName, type = typeof(SkinnedMeshRenderer) }, 0, blendShape.Max);

            @default.Add(new() { path = facePath, propertyName = propertyName, type = typeof(SkinnedMeshRenderer) }, 0, blendShape.Value);

            var paramNameBase = $"{ParameterNames.Internal.BlendShapes.Prefix}{name}";

            BlendTreeBuilder parent;

            if (!usageInfo.AllowControl)
            {
                if (!usageInfo.UseOverrideGate)
                    continue;

                var overrideTree = blendTree.AddBlendTree("Override").Motion;
                overrideTree.BlendParameter = $"{paramNameBase}/Override";
                animatorController.Parameters.AddFloat(overrideTree.BlendParameter);

                overrideTree.Append(min, threshold: float.Epsilon);
                overrideTree.Append(max, threshold: 1);
                continue;
            }

            if (usageInfo.UseEnableGate)
            {
                var enableSwitch = blendTree.AddBlendTree($"{name}").Motion;
                enableSwitch.BlendParameter = $"{paramNameBase}/Enable";
                enableSwitch.Append(data.BlankClip, threshold: 0);
                parent = enableSwitch;

                animatorController.Parameters.AddFloat(enableSwitch.BlendParameter, 1);
            }
            else
            {
                parent = blendTree;
            }

            float nextThreshold;

            if (usageInfo.UseOverrideGate)
            {
                var overrideTree = parent.AddBlendTree("Override").WithThreshold(1).Motion;
                overrideTree.BlendParameter = $"{paramNameBase}/Override";
                parent = overrideTree;
                animatorController.Parameters.AddFloat(overrideTree.BlendParameter);

                overrideTree.Append(min, threshold: float.Epsilon);
                overrideTree.Append(max, threshold: 1);
                nextThreshold = 0;
            }
            else
            {
                nextThreshold = 1;
            }

            if (usageInfo.UseCancelGate)
            {
                var layerIndex = BinaryUtils.PopIndex(usageInfo.CancelGateFlags, bitBuffer);
                for (int i = 0; i < layerIndex.Length; i++)
                {
                    var cancelTree = parent.AddBlendTree("Cancel").WithThreshold(nextThreshold).Motion;
                    cancelTree.BlendParameter = $"{paramNameBase}/Cancel/{layerIndex[i]}";

                    cancelTree.Append(min, threshold: 1);

                    parent = cancelTree;
                    animatorController.Parameters.AddFloat(cancelTree.BlendParameter);
                    nextThreshold = 0;
                }
            }

            if (usageInfo.UseControlGate)
            {
                var layerIndex = BinaryUtils.PopIndex(usageInfo.ControlGateLayers, bitBuffer);
                for (int i = 0; i < layerIndex.Length; i++)
                {
                    var controlTree = parent.AddBlendTree("Control").WithThreshold(nextThreshold).Motion;
                    controlTree.BlendParameter = $"{paramNameBase}/Value/{layerIndex[i]}";

                    controlTree.Append(max, threshold: 1);

                    parent = controlTree;
                    animatorController.Parameters.AddFloat(controlTree.BlendParameter);
                    nextThreshold = 0;
                }

                parent.Append(min, nextThreshold);
            }
            else
            {
                parent.Append(@default, nextThreshold);
            }
        }
    }

    public static void GenerateBlendshapeSyncController(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        var modEmo = context.GetModEmoContext().Root;
        var data = context.GetData();

        if (modEmo.RuntimeBlendshapeController is not { Sync: true } rbc)
            return;

        var layer = animatorController.AddLayer("[ModEmo] BlendShape Sync");
        var stateMachine = layer.StateMachine.WithDefaultWriteDefaults(true).WithDefaultMotion(data.BlankClip);

        var entryState = stateMachine.AddState("Entry");

        animatorController.AddParameter(ParameterNames.Internal.BlendShapes.Sync.Selected, AnimatorControllerParameterType.Int);
        animatorController.AddParameter(ParameterNames.Internal.BlendShapes.Sync.Index, AnimatorControllerParameterType.Int);
        animatorController.AddParameter(ParameterNames.Internal.BlendShapes.Sync.Value, AnimatorControllerParameterType.Float);
        data.Parameters.Add(new(ParameterNames.Internal.BlendShapes.Sync.Selected, 0, AnimatorParameterType.Int, false, true));
        data.Parameters.Add(new(ParameterNames.Internal.BlendShapes.Sync.Index, 0, AnimatorParameterType.Int, false, false));
        data.Parameters.Add(new(ParameterNames.Internal.BlendShapes.Sync.Value, 0f, AnimatorParameterType.Float, false, false));

        var localIdle = stateMachine.AddState("[LOCAL] Idle");
        entryState.AddTransition(localIdle).If("IsLocal");

        var remoteIdle = stateMachine.AddState("[REMOTE] Idle");
        entryState.AddTransition(remoteIdle).IfNot("IsLocal");

        foreach (var x in data.FaceInfo.BlendShapes)
        {
            if (!x.UsageInfo.UseOverrideGate)
                continue;

            var index = data.GetBlendshapeIndexForSync(x.Index);

            // Local
            {
                var idle = localIdle;
                var state = stateMachine.AddState($"Send {x.Name}");
                idle.AddTransition(state).Equals(ParameterNames.Internal.BlendShapes.Sync.Selected, index);
                state.AddTransition(idle).NotEqual(ParameterNames.Internal.BlendShapes.Sync.Selected, index);

                state.AddTransition(idle).Equals(ParameterNames.Internal.BlendShapes.Sync.Selected, index).WithExitTime(0.05f).WithDuration(0);

                state.AddAvatarParameterDriver()
                    .Set(ParameterNames.Internal.BlendShapes.Sync.Index, index)
                    .Copy($"{ParameterNames.Internal.BlendShapes.Prefix}{x.Name}/Override", ParameterNames.Internal.BlendShapes.Sync.Value);
            }

            //Remote
            {
                var idle = remoteIdle;
                var state = stateMachine.AddState($"Receive {x.Name}");
                idle.AddTransition(state).Equals(ParameterNames.Internal.BlendShapes.Sync.Index, index);
                state.AddTransition(idle).NotEqual(ParameterNames.Internal.BlendShapes.Sync.Index, index);

                state.AddTransition(idle).Equals(ParameterNames.Internal.BlendShapes.Sync.Index, index).WithExitTime(0.05f).WithDuration(0);

                state.AddAvatarParameterDriver()
                    .Copy(ParameterNames.Internal.BlendShapes.Sync.Value, $"{ParameterNames.Internal.BlendShapes.Prefix}{x.Name}/Override");
            }

        }

        //Reset

        var resetWait = stateMachine.AddState("[LOCAL] Reset Idle");
        localIdle.AddTransition(resetWait).Greater(ParameterNames.Internal.BlendShapes.Reset, 0);
        var resetStart = stateMachine.AddState("[LOCAL] Reset Start");
        resetWait.AddTransition(resetStart).Equals(ParameterNames.Internal.BlendShapes.Reset, 0);

        var resetStateParent = resetStart;

        foreach (var x in data.FaceInfo.BlendShapes)
        {
            if (!x.UsageInfo.UseOverrideGate)
                continue;

            var index = data.GetBlendshapeIndexForSync(x.Index);

            var state = stateMachine.AddState($"[LOCAL] Reset {x.Name}");
            resetStateParent.AddTransition(state).WithExitTime(0.5f);
            state.AddAvatarParameterDriver()
                .Set(ParameterNames.Internal.BlendShapes.Sync.Index, index)
                .Set(ParameterNames.Internal.BlendShapes.Sync.Value, 0);
            resetStateParent = state;
        }

        var resetEnd = stateMachine.AddState("[LOCAL] Reset End");
        resetEnd.AddAvatarParameterDriver()
            .Set(ParameterNames.Internal.BlendShapes.Sync.Index, 0)
            .Set(ParameterNames.Internal.BlendShapes.Sync.Value, 0);
        resetStateParent.AddTransition(resetEnd).WithExitTime(0.5f);
        resetEnd.AddTransition(localIdle).Equals(ParameterNames.Internal.BlendShapes.Reset, 0);
    }
}
