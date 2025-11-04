using Numeira.Animation;

namespace Numeira;

internal static class GestureWeightSmootherGenerator
{
    public static VirtualLayer Generate(BuildContext context)
    {
        var vcc = context.GetVirtualControllerContext();

        var layer = VirtualLayer.Create(vcc.CloneContext, "[ModEmo] Gesture Weight Smoother");
        var tree = new DirectBlendTree();

        var data = context.GetData();
        data.Parameters.Add(new("GestureLeftWeight", 0f));
        data.Parameters.Add(new("GestureRightWeight", 0f));
        data.Parameters.Add(new($"{ParameterNames.Internal.Input.LeftWeight}", 0f));
        data.Parameters.Add(new($"{ParameterNames.Internal.Input.RightWeight}", 0f));

        foreach (var side in new[] { "Left", "Right" })
        {
            var a = tree.AddBlendTree(side);
            a.BlendParameter = ParameterNames.Internal.SmoothAmount;
            var b1 = a.AddBlendTree("");
            b1.BlendParameter = $"Gesture{side}Weight";
            var b2 = a.AddBlendTree("");
            b2.BlendParameter = $"{ParameterNames.Internal.Input.Prefix}{side}/Weight";

            var min = new AnimationClip() { name = "Min" };
            var max = new AnimationClip() { name = "Max" };
            var bind = AnimationUtils.CreateAAPBinding(b2.BlendParameter);
            AnimationUtility.SetEditorCurve(min, bind, AnimationCurve.Constant(0, 0, 0));
            AnimationUtility.SetEditorCurve(max, bind, AnimationCurve.Constant(0, 0, 1));

            b1.AddMotion(min);
            b1.AddMotion(max);
            b2.AddMotion(min);
            b2.AddMotion(max);
        }

        layer.StateMachine!.AddState("DirectBlendTree (WD On)", vcc.Clone(tree.Build(context.AssetContainer)));
        return layer;
    }

    public static void Generate(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        foreach (var x in context.AvatarRootObject.GetComponentsInChildren<ModEmoMotionTime>())
        {
            if (x.ParameterName is "GestureLeftWeight")
                x.ParameterName = ParameterNames.Internal.Input.LeftWeight;
            else if (x.ParameterName is "GestureRightWeight")
                x.ParameterName = ParameterNames.Internal.Input.RightWeight;
        }

        var layer = animatorController.AddLayer("[ModEmo] Gesture Weight Smoother");
        DirectBlendTreeBuilder tree = new DirectBlendTreeBuilder() { DefaultDirectBlendParameter = ParameterNames.Internal.One };
        layer.StateMachine.WithDefaultMotion(tree).AddState("DirectBlendTree (WD On)");

        animatorController.Parameters.AddFloat("GestureLeftWeight", 0f);
        animatorController.Parameters.AddFloat("GestureRightWeight", 0f);
        animatorController.Parameters.AddFloat($"{ParameterNames.Internal.Input.LeftWeight}", 0f);
        animatorController.Parameters.AddFloat($"{ParameterNames.Internal.Input.RightWeight}", 0f);

        foreach (var side in new[] { "Left", "Right" })
        {
            var a = tree.AddBlendTree(side).Motion;
            a.BlendParameter = ParameterNames.Internal.SmoothAmount;
            var b1 = a.AddBlendTree("").Motion;
            b1.BlendParameter = $"Gesture{side}Weight";
            var b2 = a.AddBlendTree("").Motion;
            b2.BlendParameter = $"{ParameterNames.Internal.Input.Prefix}{side}/Weight";

            var min = new AnimationClipBuilder() { Name = "Min" };
            var max = new AnimationClipBuilder() { Name = "Max" };
            min.AddAnimatedParameter(b2.BlendParameter, 0, 0);
            max.AddAnimatedParameter(b2.BlendParameter, 0, 1);

            b1.Append(min);
            b1.Append(max);
            b2.Append(min);
            b2.Append(max);
        }

    }
}