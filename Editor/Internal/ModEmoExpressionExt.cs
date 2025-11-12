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
        var blendShapes = data.BlendShapes;
        var usageBlendShapes = data.UsageBlendShapeMap;

        if (writeDefaultValues)
        {
            foreach (var (name, blendShape) in blendShapes)
            {
                if (usageBlendShapes != null && !usageBlendShapes.Contains(name))
                    continue;

                float value = blendShape.Value;
                value /= blendShape.Max;

                anim.AddAnimatedParameter($"{ParameterNames.Internal.BlendShapes.Prefix}{name}/Value", 0, value);
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
            bool isCancel = false;
            if (name.StartsWith(cancelShapeNamePrefix))
            {
                isCancel = true;
                name = name[cancelShapeNamePrefix.Length..];
            }

            if (!name.StartsWith(blendShapeNamePrefix))
                return;

            name = name[blendShapeNamePrefix.Length..];

            binding = new(typeof(Animator), "", $"{ParameterNames.Internal.BlendShapes.Prefix}{name}{(isCancel ? "/Cancel" : "/Value")}");

            if (!blendShapes.TryGetValue(name.ToString(), out var defaultValue))
                defaultValue = new(0, 100);

            keyframe.Value /= defaultValue.Max;
        });

        expression.CollectAnimation(animationWriter, new(context.AvatarRootTransform, data.Face.transform, data.Face.transform.AvatarRootPath()));

        return anim;
    }

    public static string GetID<T>(this T expression) where T : IModEmoExpression
    {
        return $"{expression.Name}-{expression.GetHashCode()}";
    }
}
