using System.Collections.Immutable;
using static Numeira.ModEmoData;

namespace Numeira;

internal static class ModEmoExpressionExt
{
    public static AnimationClip MakeAnimationClip(this IModEmoExpression expression, ImmutableDictionary<string, BlendShapeInfo> blendShapes, AnimationClip? source = null, HashSet<string>? whiteList = null, bool forPreviewMode = false)
    {
        var generator = new AnimationClipGenerator() { Name = source == null ? expression.Name : $"{source.name} + {expression.Name}" };
        if (source != null)
        {
            foreach (var bind in AnimationUtility.GetCurveBindings(source))
            {
                if (!blendShapes.TryGetValue(bind.propertyName, out var value))
                    continue;

                if (whiteList != null && !whiteList.Contains(bind.propertyName))
                    continue;

                var curve = AnimationUtility.GetEditorCurve(source, bind);
                var keys = curve.keys;
                if (!keys.Any(x => x.value != value.Value))
                    continue;

                foreach (var x in keys)
                {
                    var bind2 = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = forPreviewMode ? $"{bind.propertyName}" : $"{ParameterNames.BlendShapes.Prefix}{bind.propertyName}" };

                    generator.Add(bind, x.time, x.value);
                }
            }
        }
        else
        {
            if (whiteList != null)
            {
                foreach (var blendShape in whiteList)
                {
                    var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = forPreviewMode ? $"{blendShape}" : $"{ParameterNames.BlendShapes.Prefix}{blendShape}" };

                    if (!blendShapes.TryGetValue(blendShape, out var defaultValue))
                        defaultValue = new BlendShapeInfo(0, 100);

                    generator.Add(bind, 0, defaultValue.Value / defaultValue.Max);
                }
            }
        }

        foreach (var frame in expression.Frames)
        {
            foreach (var blendShape in frame.BlendShapes)
            {
                if (forPreviewMode)
                {
                    var bind = AnimationUtils.CreateAAPBinding($"{blendShape.Name}");
                    if (!blendShapes.TryGetValue(blendShape.Name, out var defaultValue))
                        defaultValue = new BlendShapeInfo(0, 100);

                    float value;
                    if (!blendShape.Cancel)
                    {
                        value = blendShape.Value;
                    }
                    else
                    {
                        value = MathF.Max(0, MathF.Min(defaultValue.Max, defaultValue.Value - (defaultValue.Value * (blendShape.Value / defaultValue.Max))));
                    }

                    generator.Add(bind, frame.Keyframe, value);
                }
                else
                {
                    var bind = AnimationUtils.CreateAAPBinding($"{ParameterNames.BlendShapes.Prefix}{blendShape.Name}");
                    if (blendShape.Cancel)
                        bind = AnimationUtils.CreateAAPBinding($"{ParameterNames.Internal.BlendShapes.DisablePrefix}{blendShape.Name}");
                    if (!blendShapes.TryGetValue(blendShape.Name, out var defaultValue))
                        defaultValue = new BlendShapeInfo(0, 100);

                    generator.Add(bind, frame.Keyframe, blendShape.Value / defaultValue.Max);
                }
            }


            if (frame.Publisher is { } publisher && publisher.GameObject?.GetComponent<IModEmoExpressionBlinkControl>() is { } blinkCtrl)
            {
                var bind = AnimationUtils.CreateAAPBinding(ParameterNames.Blink.Disable);
                generator.Add(bind, frame.Keyframe, blinkCtrl.Enable ? 0 : 1);
            }
        }

        return generator.Export();
    }

    public static AnimationClip MakeDirectAnimationClip(this IModEmoExpression expression, string? path = null)
    {
        var generator = new AnimationClipGenerator() { Name = expression.Name };

        foreach (var frame in expression.Frames)
        {
            foreach (var blendShape in frame.BlendShapes)
            {
                generator.Add(new EditorCurveBinding() { type = typeof(SkinnedMeshRenderer), path = path, propertyName = $"blendShape.{blendShape.Name}" }, frame.Keyframe, blendShape.Value);
            }
        }

        return generator.Export();
    }
}
