using Numeira.Animation;

namespace Numeira;

internal static class GestureWeightSmootherGenerator
{
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
            var min = new AnimationClipBuilder() { Name = "Min" };
            var max = new AnimationClipBuilder() { Name = "Max" };
            
            var a = tree.AddBlendTree(side).Motion;
            a.BlendParameter = ParameterNames.Internal.SmoothAmount;

            var fistSwitch = a.AddBlendTree("Fist").Motion;
            fistSwitch.BlendParameter = $"Gesture{side}";
            fistSwitch.Append(min, threshold: 0);
            

            var b1 = fistSwitch.AddBlendTree("").WithThreshold(1).Motion;
            b1.BlendParameter = $"Gesture{side}Weight";
            var b2 = a.AddBlendTree("").Motion;
            b2.BlendParameter = $"{ParameterNames.Internal.Input.Prefix}{side}/Weight";

            min.AddAnimatedParameter(b2.BlendParameter, 0, 0);
            max.AddAnimatedParameter(b2.BlendParameter, 0, 1);

            b1.Append(min);
            b1.Append(max);
            b2.Append(min);
            b2.Append(max);

            fistSwitch.Append(min, threshold: 2);
        }

    }
}