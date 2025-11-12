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

        foreach (var (name, blendShape) in data.BlendShapes)
        {
            if (!data.UsageBlendShapeMap.TryGetValue(name, out var usageInfo))
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

            var overrideTree = parent.AddBlendTree("Override").WithThreshold(1).Motion;
            overrideTree.BlendParameter = $"{paramNameBase}/Override";
            parent = overrideTree;
            animatorController.Parameters.AddFloat(overrideTree.BlendParameter);

            overrideTree.Append(min, threshold: float.Epsilon);
            overrideTree.Append(max, threshold: 1);

            if (usageInfo.UseCancelGate)
            {
                var cancelTree = overrideTree.AddBlendTree("Cancel").WithThreshold(0).Motion;
                cancelTree.BlendParameter = $"{paramNameBase}/Cancel";

                cancelTree.Append(min, threshold: 1);

                parent = cancelTree;
                animatorController.Parameters.AddFloat(cancelTree.BlendParameter);
            }

            var controlTree = parent.AddBlendTree("Control").WithThreshold(0).Motion;
            controlTree.BlendParameter = $"{paramNameBase}/Value";

            controlTree.Append(min, threshold: 0);
            controlTree.Append(max, threshold: 1);

            animatorController.Parameters.AddFloat(controlTree.BlendParameter);
        }
    }
}
