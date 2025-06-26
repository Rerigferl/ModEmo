namespace Numeira;

internal static class ExpressionControllerGenerator
{
    public static VirtualLayer Generate(BuildContext context)
    {
        var vcc = context.GetVirtualControllerContext();
        var layer = new AnimatorControllerLayer() { name = "[ModEmo] Expressions" };
        var stateMachine = layer.stateMachine = new();

        var modEmo = context.GetModEmoContext().Root;
        var data = context.GetData();

        var expressionPatterns = modEmo.ExportExpressions();
        var usageMap = expressionPatterns
            .SelectMany(x => x)
            .SelectMany(x => x.Frames)
            .SelectMany(x => x.BlendShapes)
            .Where(x => data.BlendShapes.TryGetValue(x.Name, out var value) && value.Value != x.Value)
            .Select(x => x.Name)
            .ToHashSet();

        data.Parameters.Add(new(ParameterNames.Expression.Pattern, 0));

        var registeredClips = new AnimationData[65];

        foreach ( var (pattern, patternIdx) in expressionPatterns.Index() )
        {
            var defState = stateMachine.AddState(pattern.Key.Name);
            if (pattern.Key.DefaultExpression is null)
            {
                var clip = new AnimationClip() { name = pattern.Key.Name };
                defState.motion = clip;
                registeredClips[1].Clip = clip;

                foreach (var blendShape in usageMap)
                {
                    var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{blendShape}" };
                    if (!data.BlendShapes.TryGetValue(blendShape, out var defaultValue))
                        defaultValue = new ModEmoData.BlendShapeInfo(0, 100);
                    AnimationUtility.SetEditorCurve(clip, bind, AnimationCurve.Constant(0, 0, defaultValue.Value / defaultValue.Max /* TODO: デフォルト値を入れる */));
                }
            }
            {
                var t = stateMachine.AddAnyStateTransition(defState);
                t.canTransitionToSelf = false;
                t.duration = 0.1f;
                t.hasFixedDuration = true;
                t.hasExitTime = false;
                t.AddCondition(AnimatorConditionMode.Equals, patternIdx, ParameterNames.Expression.Pattern);
                t.AddCondition(AnimatorConditionMode.Greater, 1 - 0.01f, ParameterNames.Expression.Index);
                t.AddCondition(AnimatorConditionMode.Less, 1 + 0.01f, ParameterNames.Expression.Index);
            }

            var registeredIndexes = new HashSet<int>();
            var expressions = ((IEnumerable<IModEmoExpression>)pattern);

            registeredClips.AsSpan().Clear();

            foreach (var expression in expressions.Where(x => x.Mode == ExpressionMode.Default))
            {
                var indexes = expression.Condition.ToExpressionIndexes();

                AnimationClip? clip = null;

                foreach (var index in indexes)
                {
                    _ = index < registeredClips.Length;
                    if (registeredClips[index].Clip != null)
                        continue;

                    clip ??= MakeAnimationClip(expression);
                    registeredClips[index].Clip = clip;
                    registeredClips[index].Weight = expression.UseGestureWeight;
                }
            }

            foreach (var expression in expressions.Where(x => x.Mode == ExpressionMode.Combine))
            {
                // TODO: これ右手+右手みたいなアニメーション生成しない？

                var indexes = expression.Condition.ToExpressionIndexes();

                AnimationClip? clip = null;
                Dictionary<AnimationClip, AnimationClip> combinedCache = new();

                foreach (var index in indexes)
                {
                    _ = index < registeredClips.Length;
                    ref var registered = ref registeredClips.AsSpan()[index].Clip;
                    if (registered != null)
                    {
                        if (!combinedCache.TryGetValue(registered, out var combined))
                        {
                            combined = MakeAnimationClip(expression, registered);
                            combinedCache.Add(registered, combined);
                        }

                        registered = combined;
                    }
                    else
                    {
                        clip ??= MakeAnimationClip(expression);
                        registered = clip;
                    }
                }
            }

            AnimationClip MakeAnimationClip(IModEmoExpression expression, AnimationClip? source = null)
            {
                var generator = new AnimationClipGenerator() { Name = source == null ? expression.Name : $"{source.name} + {expression.Name}" };
                if (source != null)
                {
                    foreach (var bind in AnimationUtility.GetCurveBindings(source))
                    {
                        if (!data.BlendShapes.TryGetValue(bind.propertyName, out var value))
                            continue;

                        var curve = AnimationUtility.GetEditorCurve(source, bind);
                        var keys = curve.keys;
                        if (!keys.Any(x => x.value != value.Value))
                            continue;

                        foreach(var x in keys)
                        {
                            var bind2 = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{bind.propertyName}" };

                            generator.Add(bind, x.time, x.value);
                        }
                    }
                }
                else
                {
                    foreach (var blendShape in usageMap)
                    {
                        var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{blendShape}" };

                        if (!data.BlendShapes.TryGetValue(blendShape, out var defaultValue))
                            defaultValue = new ModEmoData.BlendShapeInfo(0, 100);

                        generator.Add(bind, 0, defaultValue.Value / defaultValue.Max);
                    }
                }

                foreach(var frame in expression.Frames)
                {
                    foreach(var blendShape in frame.BlendShapes)
                    {
                        var bind = AnimationUtils.CreateAAPBinding($"{ParameterNames.BlendShapes.Prefix}{blendShape.Name}");
                        if (blendShape.Cancel)
                            bind = AnimationUtils.CreateAAPBinding($"{ParameterNames.Internal.BlendShapes.DisablePrefix}{blendShape.Name}");
                        if (!data.BlendShapes.TryGetValue(blendShape.Name, out var defaultValue))
                            defaultValue = new ModEmoData.BlendShapeInfo(0, 100);

                        generator.Add(bind, frame.Keyframe, blendShape.Value / defaultValue.Max);
                    }
                }

                return generator.Export();
            }

            foreach(var x in registeredClips.Index().Where(x => x.Value.Clip != null).GroupBy(x => x.Value, x => x.Index, AnimationData.EqualityComparer.Default))
            {
                var state = stateMachine.AddState(x.Key.Clip!.name);
                state.motion = x.Key.Clip!;

                if (x.Key.Weight != default)
                {
                    state.timeParameterActive = true;
                    state.timeParameter = x.Key.Weight switch
                    {
                        Hand.Left => ParameterNames.Internal.Input.LeftWeight,
                        Hand.Right => ParameterNames.Internal.Input.RightWeight,
                        _ => default,
                    };
                }

                foreach (var index in x)
                {
                    var t = stateMachine.AddAnyStateTransition(state);
                    t.canTransitionToSelf = false;
                    t.duration = 0.1f;
                    t.hasFixedDuration = true;
                    t.hasExitTime = false;
                    t.AddCondition(AnimatorConditionMode.Equals, patternIdx, ParameterNames.Expression.Pattern);
                    t.AddCondition(AnimatorConditionMode.Greater, index - 0.01f, ParameterNames.Expression.Index);
                    t.AddCondition(AnimatorConditionMode.Less, index + 0.01f, ParameterNames.Expression.Index);
                }
            }

        }

        return vcc.Clone(layer, 0);
    }

    public static VirtualLayer? GenerateBlinkController(BuildContext context)
    {
        var vcc = context.GetVirtualControllerContext();
        var modEmo = context.GetModEmoContext().Root;
        if (modEmo.BlinkExpression == null)
            return default;

        var layer = new AnimatorControllerLayer() { name = "[ModEmo] Blink" };
        var stateMachine = layer.stateMachine = new();

        var data = context.GetData();

        var state = stateMachine.AddState("Blink");
        var clip = MakeAnimationClip(modEmo.BlinkExpression);
        state.motion = clip;
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        static AnimationClip MakeAnimationClip(IModEmoExpression expression)
        {
            var clip = new AnimationClip() { name = expression.Name };

            var frames = expression.Frames.GroupBy(x => x.Keyframe, x => x.BlendShapes, (x, y) => new ExpressionFrame(x, y.SelectMany(y => y))).ToArray();

            List<KeyValuePair<EditorCurveBinding, Keyframe>> aaa = new();

            foreach (var (key, blendshapes) in frames)
            {
                foreach (var blendShape in blendshapes)
                {
                    var bind = AnimationUtils.CreateAAPBinding($"{ParameterNames.BlendShapes.Prefix}{blendShape.Name}");
                    if (blendShape.Cancel)
                        bind = AnimationUtils.CreateAAPBinding($"{ParameterNames.Internal.BlendShapes.DisablePrefix}{blendShape.Name}");
                    float x = blendShape.Value / 100f;
                    aaa.Add(KeyValuePair.Create(bind, new Keyframe(key, x, 0, 0)));
                }
            }

            foreach (var group in aaa.GroupBy(x => x.Key, x => x.Value))
            {
                var a = group.ToArray();
                for (int i = 0; i < a.Length; i++)
                {
                    if (i > 0)
                        a[i].inTangent = Tangent(a[i - 1].time, a[i].time, a[i - 1].value, a[i].value);
                    if (i < a.Length - 1)
                        a[i].outTangent = Tangent(a[i].time, a[i + 1].time, a[i].value, a[i + 1].value);
                }
                AnimationUtility.SetEditorCurve(clip, group.Key, new AnimationCurve(a));
            }

            return clip;

            static float Tangent(float timeStart, float timeEnd, float valueStart, float valueEnd)
            {
                return (valueEnd - valueStart) / (timeEnd - timeStart);
            }
        }

        return vcc.Clone(layer, 0);
    }



    private struct AnimationData
    {
        public AnimationData(AnimationClip clip, Hand? weight)
        {
            Clip = clip;
            Weight = weight ?? default;
        }

        public AnimationClip? Clip;
        public Hand Weight;

        public sealed class EqualityComparer : IEqualityComparer<AnimationData>
        {
            public static EqualityComparer Default => new();

            bool IEqualityComparer<AnimationData>.Equals(AnimationData x, AnimationData y) => x.Clip == y.Clip;

            int IEqualityComparer<AnimationData>.GetHashCode(AnimationData obj) => obj.Clip!.GetHashCode();
        }
    }
}