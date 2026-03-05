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

        foreach (var blendShape in data.FaceInfo.BlendShapes)
        {
            var name = blendShape.Name;
            var usageInfo = blendShape.UsageInfo;
            if (!usageInfo.AllowControl)
                continue;

            var min = new AnimationClipBuilder() { Name = $"{name} Min" };
            var max = new AnimationClipBuilder() { Name = $"{name} Max" };
            //var @default = new AnimationClipBuilder() { Name = $"{name} Default" };
            var propertyName = $"blendShape.{name}";

            min.Add(new EditorCurveBinding() { path = facePath, propertyName = propertyName, type = typeof(SkinnedMeshRenderer) }, 0, 0);
            max.Add(new EditorCurveBinding() { path = facePath, propertyName = propertyName, type = typeof(SkinnedMeshRenderer) }, 0, blendShape.Max);
            //@default.Add(new() { path = facePath, propertyName = propertyName, type = typeof(SkinnedMeshRenderer) }, 0, blendShape.Value);

            var paramNameBase = $"{ParameterNames.Internal.BlendShapes.Prefix}{name}";

            BlendTreeBuilder parent;

            if (usageInfo.UseControlGate == false)
            {
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
                var cancelTree = parent.AddBlendTree("Cancel").WithThreshold(nextThreshold).Motion;
                cancelTree.BlendParameter = $"{paramNameBase}/Cancel";

                cancelTree.Append(min, threshold: 1);

                parent = cancelTree;
                animatorController.Parameters.AddFloat(cancelTree.BlendParameter);
                nextThreshold = 0;
            }

            var controlTree = parent.AddBlendTree("Control").WithThreshold(nextThreshold).Motion;
            controlTree.BlendParameter = $"{paramNameBase}/Value";

            controlTree.Append(min, threshold: 0);
            controlTree.Append(max, threshold: 1);

            animatorController.Parameters.AddFloat(controlTree.BlendParameter);
        }
    }
}
