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
        var usageMap = expressionPatterns.SelectMany(x => x).SelectMany(x => x.Frames).SelectMany(x => x.BlendShapes).Select(x => x.Name).ToHashSet();

        data.Parameters.Add(new(ParameterNames.Expression.Pattern, 0));

        AnimationClip[] registeredClips = new AnimationClip[65];

        foreach ( var (pattern, patternIdx) in expressionPatterns.Index() )
        {
            var defState = stateMachine.AddState(pattern.Key.Name);
            if (pattern.Key.DefaultExpression is null)
            {
                var clip = new AnimationClip() { name = pattern.Key.Name };
                defState.motion = clip;
                registeredClips[1] = clip;

                foreach (var blendShape in usageMap)
                {
                    var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{blendShape}" };
                    AnimationUtility.SetEditorCurve(clip, bind, AnimationCurve.Constant(0, 0, 0 /* TODO: デフォルト値を入れる */));
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
                    if (registeredClips[index] != null)
                        continue;

                    clip ??= MakeAnimationClip(expression);
                    registeredClips[index] = clip;
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
                    ref var registered = ref registeredClips.AsSpan()[index];
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
                var clip = new AnimationClip() { name = source == null ? expression.Name : $"{source.name} + {expression.Name}" };

                if (source != null)
                {
                    foreach(var bind in AnimationUtility.GetCurveBindings(source))
                    {
                        var curve = AnimationUtility.GetEditorCurve(source, bind);
                        AnimationUtility.SetEditorCurve(clip, bind, curve);
                    }
                }
                else
                {
                    foreach (var blendShape in usageMap)
                    {
                        var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{blendShape}" };
                        AnimationUtility.SetEditorCurve(clip, bind, AnimationCurve.Constant(0, 0, 0 /* TODO: デフォルト値を入れる */));
                    }
                }

                foreach (var (key, blendshapes) in expression.Frames.GroupBy(x => x.Keyframe, x => x.BlendShapes, (x, y) => new ExpressionFrame(x, y.SelectMany(y => y))))
                {
                    foreach (var blendShape in blendshapes)
                    {
                        var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{blendShape.Name}" };
                        AnimationUtility.SetEditorCurve(clip, bind, AnimationCurve.Constant(key, key, blendShape.Value / 100f /* TODO: ちゃんとウェイト調べる */));
                    }
                }
                return clip;
            }

            foreach(var x in registeredClips.Index().Where(x => x.Value != null).GroupBy(x => x.Value, x => x.Index))
            {
                var state = stateMachine.AddState(x.Key.name);
                state.motion = x.Key;

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
}