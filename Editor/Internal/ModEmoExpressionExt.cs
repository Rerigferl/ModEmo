using System.Buffers.Text;
using System.Collections.Immutable;
using Numeira.Animation;

namespace Numeira;

internal static class ModEmoExpressionExt
{
    public static AnimationClipBuilder MakeAnimationClip<T>(this T expression, ModEmoData data, bool writeDefault = true, bool writeBlink = true) where T : IModEmoExpression
        => expression.MakeAnimationClip(data.BlendShapes, data.UsageBlendShapeMap, writeDefault, writeBlink);

    public static AnimationClipBuilder MakeAnimationClip<T>(
        this T expression,
        ImmutableDictionary<string, BlendShapeInfo> blendShapes,
        ImmutableHashSet<string>? usageBlendShapes,
        bool writeDefault = true,
        bool writeBlink = true,
        bool previewMode = false) where T : IModEmoExpression
    {
        var anim = new AnimationClipBuilder
        {
            Name = expression.Name,
            IsLoop = expression.IsLoop
        };

        if (writeDefault)
        {
            foreach (var (name, blendShape) in blendShapes)
            {
                if (usageBlendShapes != null && !usageBlendShapes.Contains(name))
                    continue;

                float value = blendShape.Value;
                if (!previewMode)
                    value /= blendShape.Max;

                anim.AddAnimatedParameter(previewMode ? name : $"{ParameterNames.Internal.BlendShapes.Prefix}{name}/Value", 0, value);
            }

            if (writeBlink && !previewMode)
            {
                anim.AddAnimatedParameter(ParameterNames.Blink.Value, 0, 1);
            }
        }

        if (!previewMode)
            anim.AddAnimatedParameter(ParameterNames.MouthMorphCancel.Enable, 0, expression.EnableMouthMorphCancel ? 1 : 0);

        foreach(var blendShape in expression.BlendShapes)
        {
            var name = blendShape.Name;
            if (!blendShapes.TryGetValue(name, out var defaultValue))
                defaultValue = new(0, 100);

            foreach(var key in blendShape.Value.keys)
            {
                float value = (previewMode, blendShape.Cancel) switch
                {
                    (true, false) => Mathf.Clamp(key.value, 0, defaultValue.Max),
                    (true, true) => Mathf.Clamp(defaultValue.Value * (1 - key.value / defaultValue.Max), 0, defaultValue.Max),
                    _ => key.value / defaultValue.Max,
                };

                anim.AddAnimatedParameter(previewMode ? name : $"{ParameterNames.Internal.BlendShapes.Prefix}{name}{(blendShape.Cancel ? "/Cancel" : "/Value")}", key.time, value);
            }

        }

        return anim;
    }

    public static string GetID<T>(this T expression) where T : IModEmoExpression
    {
        return $"{expression.Name}-{expression.GetHashCode()}";
    }
}
