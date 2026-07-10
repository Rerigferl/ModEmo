using System.Collections.Immutable;
using nadena.dev.ndmf.util;
using Numeira.Animation;

namespace Numeira;

internal static class ModEmoExpressionExt
{
    public static AnimationClipBuilder MakeAnimationClip<T>(this T expression, BuildContext context, bool writeDefaultValues = true, bool writeBlink = true) where T : IModEmoExpression
    {
        var anim = new AnimationClipBuilder
        {
            Name = expression.Name,
            IsLoop = expression.IsLoop
        };

        var data = context.GetData();

        if (writeDefaultValues)
        {
            foreach (var blendShape in data.FaceInfo.BlendShapes)
            {
                if (!blendShape.UsageInfo.AllowControl)
                    continue;

                float value = blendShape.Value;
                value /= blendShape.Max;

                anim.AddAnimatedParameter(data.FaceInfo.RegisterControlBlendshape(blendShape.Name, BlendshapeControlType.Normal, expression.LayerIndex) ?? "", 0, value);
            }

            if (writeBlink)
            {
                anim.AddAnimatedParameter(ParameterNames.Blink.Value, 0, 1);
            }
        }

        var animationWriter = new AnimationClipBuilderWriter(anim);

        using var __ = animationWriter.RegisterPreWriteKeyframe((ref AnimationBinding binding, ref Curve.Keyframe keyframe) =>
        {
            if (binding.Type != typeof(SkinnedMeshRenderer))
                return;

            const string cancelShapeNamePrefix = "cancel.";
            const string blendShapeNamePrefix = "blendShape.";
            var name = binding.PropertyName.AsSpan();
            BlendshapeControlType type = BlendshapeControlType.Normal;

            if (name.StartsWith(cancelShapeNamePrefix))
            {
                type = BlendshapeControlType.Cancel;
                name = name[cancelShapeNamePrefix.Length..];
            }

            if (!name.StartsWith(blendShapeNamePrefix))
                return;

            name = name[blendShapeNamePrefix.Length..];
            var nameStr = name.ToString();
            binding = new(typeof(Animator), "", data.FaceInfo.RegisterControlBlendshape(nameStr, type, expression.LayerIndex) ?? "");

            float maxValue = 100;
            if (data.FaceInfo.BlendshapeMap.TryGetValue(nameStr, out var info))
            {
                maxValue = info.Max;
            }

            keyframe.Value /= maxValue;
        });

        expression.CollectAnimation(animationWriter, new(context.AvatarRootTransform, data.Face.transform, data.Face.transform.AvatarRootPath()));

        if (expression.EnableMouthMorphCancel)
        {
            anim.AddAnimatedParameter($"{ParameterNames.Internal.MouthMorphCancel.Enable}", 0, 1);
        }

        return anim;
    }

    public static string GetID<T>(this T expression) where T : IModEmoExpression
    {
        return $"{expression.Name}-{expression.GetHashCode()}";
    }
}
