using System.Collections.Immutable;
using System.Runtime.Remoting.Contexts;
using nadena.dev.ndmf.util;
using Numeira.Animation;
using UnityEngine.UIElements;

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

        var animationWriter = new AnimationClipBuilderWriter(anim);

        if (writeDefaultValues)
        {
            animationWriter.WriteDefaultValues(expression, data.FaceInfo, writeBlink);
        }


        using var __ = animationWriter.RegisterPreWriteKeyframe(PreWriteKeyframeContext.From(expression, context).GeneralPreWriteKeyframe);

        expression.CollectAnimation(animationWriter, new(context.AvatarRootTransform, data.Face.transform, data.Face.transform.AvatarRootPath()));

        if (expression.EnableMouthMorphCancel)
        {
            anim.AddAnimatedParameter($"{ParameterNames.Internal.MouthMorphCancel.Enable}", 0, 1);
        }

        return anim;
    }

    public static MotionBuilder ToMotion<T>(this T expression, BuildContext context, bool writeDefaultValues = true, bool writeBlink = true) where T : IModEmoExpression
    {
        var motionTime = expression.MotionTime.ToArray();
        var data = context.GetData();
        MotionBuilder result;
        switch (motionTime.Length)
        {
            case 0:
                {
                    var dbt = new DirectBlendTreeBuilder() { DefaultDirectBlendParameter = ParameterNames.Internal.One };
                    dbt.Append(expression.MakeAnimationClip(context, writeDefaultValues, writeBlink));

                    result = dbt;
                }
                break;
            case 1:
                {
                    var writer = new TimeSeparatedAnimationClipBuilderWriter(time => new() { Name = $"{expression.Name}: {time}", IsLoop = expression.IsLoop });
                    writer.RegisterPreWriteKeyframe(PreWriteKeyframeContext.From(expression, context).GeneralPreWriteKeyframe);

                    if (writeDefaultValues)
                    {
                        writer.WriteDefaultValues(expression, data.FaceInfo, writeBlink);
                    }

                    expression.CollectAnimation(writer, new(context.AvatarRootTransform, data.Face.transform, data.Face.transform.AvatarRootPath()));

                    var bt = new OneDirectionBlendTreeBuilder
                    {
                        Name = $"{expression.Name}",
                        BlendParameter = motionTime[0]
                    };

                    foreach (var (time, clip) in writer.GetAnimationClips())
                    {
                        bt.Append(clip, time);
                    }

                    result = bt;
                }
                break;
            case >= 2:
                {
                    var subExpressions = expression.Component.GetComponentsInDirectChildren<IModEmoExpression>(includeSelf: false);
                    if (subExpressions.Length < 2)
                        throw new Exception("");
                    /*
                    var dbt = new DirectBlendTreeBuilder()
                    {
                        Name = $"{expression.Name}",
                        NormalizedBlendValues = true,
                    };

                    dbt.Append(expression.MakeAnimationClip(context, false), directBlendParameter: ParameterNames.Internal.One);
                    for (int i = 0; i < Math.Min(subExpressions.Length, motionTime.Length); i++)
                    {
                        dbt.Append(subExpressions[i].MakeAnimationClip(context, false), directBlendParameter: motionTime[i]);
                    }
                    result = dbt;
                    */

                    var bt = new TwoDirectionBlendTreeBuilder()
                    {
                        Name = $"{expression.Name}",
                        BlendParameterX = motionTime[0],
                        BlendParameterY = motionTime[1],
                        IsFreeform = true,
                        IsCertein = true,
                    };

                    bt.Append(expression.MakeAnimationClip(context, false, writeBlink), position: new(0, 0));
                    bt.Append(subExpressions[0].MakeAnimationClip(context, false, writeBlink), position: new(1, 0));
                    bt.Append(subExpressions[1].MakeAnimationClip(context, false, writeBlink), position: new(0, 1));
                    if (subExpressions.Length < 3)
                    {
                        bt.Append(Merge(subExpressions[0], subExpressions[1]), position: new(1, 1));
                    }
                    else
                    {
                        bt.Append(subExpressions[2].MakeAnimationClip(context, false, writeBlink), position: new(1, 1));
                    }

                    result = bt;

                    AnimationClipBuilder Merge(IModEmoExpression e1, IModEmoExpression e2)
                    {
                        var anim = new AnimationClipBuilder
                        {
                            Name = $"{expression.Name}: {e1.Name} + {e2.Name}",
                            IsLoop = expression.IsLoop
                        };

                        var animationWriter = new AnimationClipBuilderWriter(anim);

                        using var __ = animationWriter.RegisterPreWriteKeyframe(PreWriteKeyframeContext.From(expression, context).GeneralPreWriteKeyframe);

                        var ac = new AnimationWriterContext(context.AvatarRootTransform, data.Face.transform, data.Face.transform.AvatarRootPath());
                        e1.CollectAnimation(animationWriter, ac);
                        e2.CollectAnimation(animationWriter, ac);

                        if (expression.EnableMouthMorphCancel)
                        {
                            anim.AddAnimatedParameter($"{ParameterNames.Internal.MouthMorphCancel.Enable}", 0, 1);
                        }

                        return anim;
                    }
                }
                break;
            default:
                throw new Exception();
        }

        return result;
    }

    private static void WriteDefaultValues<TWriter, TExpression>(this TWriter writer, TExpression expression, FaceInfo faceInfo, bool writeBlink = false) where TWriter : IAnimationWriter where TExpression : IModEmoExpression
    {

        foreach (var blendShape in faceInfo.BlendShapes)
        {
            if (!blendShape.UsageInfo.AllowControl)
                continue;

            float value = blendShape.Value;
            value /= blendShape.Max;

            writer.WriteDefaultValue(AnimationBinding.Parameter(faceInfo.RegisterControlBlendshape(blendShape.Name, BlendshapeControlType.Normal, expression.LayerIndex) ?? ""), value);
        }

        if (writeBlink)
        {
            writer.WriteDefaultValue(AnimationBinding.Parameter(ParameterNames.Blink.Value), 1);
        }
    }

    private sealed class PreWriteKeyframeContext
    {
        public IModEmoExpression Expression { get; set; } = null!;
        public BuildContext Context { get; set; } = null!;

        private static readonly PreWriteKeyframeContext instance = new();

        public static PreWriteKeyframeContext From(IModEmoExpression expression,  BuildContext context)
        {
            var x = instance;
            x.Expression = expression;
            x.Context = context;
            return x;
        }
    }

    private static void GeneralPreWriteKeyframe(this PreWriteKeyframeContext @this, ref AnimationBinding binding, ref Curve.Keyframe keyframe)
    {
        var data = @this.Context.GetData();

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
        binding = new(typeof(Animator), "", data.FaceInfo.RegisterControlBlendshape(nameStr, type, @this.Expression.LayerIndex) ?? "");

        float maxValue = 100;
        if (data.FaceInfo.BlendshapeMap.TryGetValue(nameStr, out var info))
        {
            maxValue = info.Max;
        }

        keyframe.Value /= maxValue;
    }

    public static string GetID<T>(this T expression) where T : IModEmoExpression
    {
        return $"{expression.Name}-{expression.GetHashCode()}";
    }
}
