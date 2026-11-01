using Numeira.Animation;
using UnityEngine.TextCore;

namespace Numeira;

internal class EyeTrackingGenerator
{
    public static void Generate(BuildContext context, AnimatorControllerBuilder animatorController, ModEmoFaceTrackingExpression expression, DirectBlendTreeBuilder root)
    {
        (IModEmoExpression? Expression, string ParameterName, float RangeStart, float RangeEnd)[] items = new[]
        {
            (expression.FindExpression(FaceTrackingMarkerRole.Left | FaceTrackingMarkerRole.Close), ParameterNames.FaceTracking.LeftEyeClose, 0f, 1f),
            (expression.FindExpression(FaceTrackingMarkerRole.Left | FaceTrackingMarkerRole.LookX), ParameterNames.FaceTracking.LeftEyeX, -1, 1),
            (expression.FindExpression(FaceTrackingMarkerRole.Left | FaceTrackingMarkerRole.LookY), ParameterNames.FaceTracking.LeftEyeX, -1, 1),

            (expression.FindExpression(FaceTrackingMarkerRole.Right | FaceTrackingMarkerRole.Close), ParameterNames.FaceTracking.RightEyeClose, 0, 1),
            (expression.FindExpression(FaceTrackingMarkerRole.Right | FaceTrackingMarkerRole.LookX), ParameterNames.FaceTracking.RightEyeX, -1, 1),
            (expression.FindExpression(FaceTrackingMarkerRole.Right | FaceTrackingMarkerRole.LookY), ParameterNames.FaceTracking.RightEyeX, -1, 1),
        };

        if (items.All(x => x.Expression is null))
            return;

        (DirectBlendTreeBuilder Input, DirectBlendTreeBuilder Output) smoothBlendTrees;

        // Smoothing
        {
            var smooth = root.AddBlendTree("Smooth").Motion;
            smooth.BlendParameter = ParameterNames.Internal.SmoothAmount;

            var input = smooth.AddDirectBlendTree("Input").WithThreshold(0).Motion;
            var output = smooth.AddDirectBlendTree("Output").WithThreshold(1).Motion;

            smoothBlendTrees = (input, output);
        }

        var parent = root.AddDirectBlendTree("Motions").Motion;
        var faceInfo = context.GetData().FaceInfo;

        foreach(var (e, parameterName, rangeStart, rangeEnd) in items)
        {
            if (e?.ToMotion(context, 1, false, false) is not { } motion)
                continue;

            var displayName = parameterName.Substring("v2/".Length);
            var smoothed = $"{parameterName}/Smooth";
            var i = smoothBlendTrees.Input.AddBlendTree(displayName).Motion;
            var aap0 = i.AddAnimationClip($"{displayName} {rangeStart}").WithThreshold(rangeStart).Motion;
            var aap1 = i.AddAnimationClip($"{displayName} {rangeEnd}").WithThreshold(rangeEnd).Motion;
            aap0.AddAnimatedParameter(smoothed, 0, rangeStart);
            aap1.AddAnimatedParameter(smoothed, 0, rangeEnd);

            animatorController.Parameters.AddFloat(parameterName, 0);
            animatorController.Parameters.AddFloat(smoothed, 0);

            var o = smoothBlendTrees.Output.AddBlendTree(displayName).Motion;
            o.Append(aap0, rangeStart);
            o.Append(aap1, rangeEnd);
            i.BlendParameter = parameterName;
            o.BlendParameter = smoothed;

            parent.Append(motion, directBlendParameter: smoothed);
        }
    }
}
