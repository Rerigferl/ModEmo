using Numeira.Animation;
using static UnityEditor.VersionControl.Asset;

namespace Numeira;
internal static class ExpressionControllerGenerator
{
    [MenuItem("Test/AsdWEQWEQe")]
    public static void A()
    {
        var ac = new AnimatorController();
        AssetDatabase.CreateAsset(ac, "Assets/_Test/A.controller");

        ac.AddLayer("a");
        var layer = ac.layers[0];
        var s = layer.stateMachine;

        var s2 = s.AddStateMachine("Hello");

        s.AddStateMachineExitTransition(s2);
    }

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

        foreach(var (pattern, patternIdx) in expressionPatterns.Index())
        {
            var mac = stateMachine.AddStateMachine(pattern.Key.Name);
        }

        /*
         * 
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
                        defaultValue = new BlendShapeInfo(0, 100);
                    AnimationUtility.SetEditorCurve(clip, bind, AnimationCurve.Constant(0, 0, defaultValue.Value / defaultValue.Max /* TODO: デフォルト値を入れる /));
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

            var gestures = Enum.GetValues(typeof(Gesture)).Cast<Gesture>();
            var maskAndIndexes = gestures.SelectMany(x => gestures.Select(y => (Left: x, Right: y)))
                .Select(x =>
                {
                    return (Mask: GestureToMask(x.Left, x.Right), Index: GestureToIndex(x.Left, x.Right));
                    static uint GestureToMask(Gesture left, Gesture right) => (0b_0000_0001_0000_0000u << (int)right | 0b_0000_0000_0000_0001u << (int)left);
                    static int GestureToIndex(Gesture left, Gesture right) => 1 + (int)(left) + (int)(right) * 8;
                })
                .ToArray();

            foreach (var expression in expressions.Where(x => x.Mode == ExpressionMode.Default))
            {
                if (expression.Settings.ConditionFolder is { } conditionFolder && conditionFolder.GetComponentsInDirectChildren<IModEmoConditionProvider>()?.FirstOrDefault() is { } condition)
                {
                    var conditionMask = condition.GetConditionMask();
                    var indexes = maskAndIndexes.Where(x => (conditionMask & x.Mask) == conditionMask).Select(x => x.Index);

                    AnimationClip? clip = null;

                    foreach (var index in indexes)
                    {
                        _ = index < registeredClips.Length;
                        if (registeredClips[index].Clip != null)
                            continue;

                        clip ??= MakeAnimationClip(expression);
                        registeredClips[index].Clip = clip;
                        registeredClips[index].TimeParameterName = expression.GameObject?.GetComponent<IModEmoMotionTimeProvider>()?.ParameterName;
                    }
                }

            }

            foreach (var expression in expressions.Where(x => x.Mode == ExpressionMode.Combine))
            {
                if (expression.Settings.ConditionFolder is { } conditionFolder && conditionFolder.GetComponentsInDirectChildren<IModEmoConditionProvider>()?.FirstOrDefault() is { } condition)
                {
                    var conditionMask = condition.GetConditionMask();
                    var indexes = maskAndIndexes.Where(x => (conditionMask & x.Mask) != 0).Select(x => x.Index);
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

                        if (!usageMap.Contains(bind.propertyName))
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
                            defaultValue = new BlendShapeInfo(0, 100);

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
                            defaultValue = new BlendShapeInfo(0, 100);

                        generator.Add(bind, frame.Keyframe, blendShape.Value / defaultValue.Max);
                    }


                    if (frame.Publisher is { } publisher && publisher.GameObject?.GetComponent<IModEmoExpressionBlinkControl>() is { } blinkCtrl)
                    {
                        var bind = AnimationUtils.CreateAAPBinding(ParameterNames.Blink.Disable);
                        generator.Add(bind, frame.Keyframe, blinkCtrl.Enable ? 0 : 1);
                    }
                }

                return generator.Export();
            }

            foreach(var x in registeredClips.Index().Where(x => x.Value.Clip != null).GroupBy(x => x.Value, x => x.Index, AnimationData.EqualityComparer.Default))
            {
                var state = stateMachine.AddState(x.Key.Clip!.name);
                state.motion = x.Key.Clip!;

                if (x.Key.TimeParameterName != default)
                {
                    state.timeParameterActive = true;
                    state.timeParameter = x.Key.TimeParameterName;
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

         */
        return vcc.Clone(layer, 0);
    }

    public static void Generate(BuildContext context, AnimatorControllerBuilder animatorController)
    {
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

        var layer = animatorController.AddLayer("[ModEmo] Expressions");
        var stateMachine = layer.StateMachine.WithDefaultMotion(data.BlankClip);

        animatorController.Parameters.AddFloat(ParameterNames.Expression.Pattern, 0);
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Index, 0);

        DirectBlendTree.DefaultDirectBlendParameter = ParameterNames.Internal.One;
        var blendTree = new DirectBlendTree();

        var patternSwitch = blendTree.AddBlendTree("Pattern Switch");
        patternSwitch.BlendParameter = ParameterNames.Expression.Pattern;
        var patternFallback = new MotionBranch(data.BlankClip);

        const float Epsilon = 0.005f;

        int expressionCount = 0;
        foreach (var (pattern, patternIdx) in expressionPatterns.Index())
        {
            var patternTree = patternSwitch.AddDirectBlendTree(pattern.Key.Name, patternIdx);

            var defM = new AnimationClip();
            AnimationUtility.SetEditorCurve(defM, AnimationUtils.CreateAAPBinding(ParameterNames.Expression.Index), AnimationCurve.Constant(0, 0, expressionCount++));

            IBlendTree? nextTree = null;

            foreach(var (expression, expressionIdx) in pattern.Index())
            {
                var expressionTree = (nextTree ?? patternTree).AddDirectBlendTree(expression.Name);
                nextTree = expressionTree;

                var expressionMotion = new AnimationClip();
                AnimationUtility.SetEditorCurve(expressionMotion, AnimationUtils.CreateAAPBinding(ParameterNames.Expression.Index), AnimationCurve.Constant(0, 0, expressionCount++));
                
                foreach (var conditionsOr in expression.Conditions)
                {
                    float? prevValue = null;
                    var fallbackTree = new DirectBlendTree() { Name = "Fallback" };
                    foreach(var conditionsAnd in conditionsOr)
                    {
                        var conditionTree = nextTree.AddBlendTree(conditionsAnd.Parameter.Name, prevValue);
                        conditionTree.BlendParameter = conditionsAnd.Parameter.Name;
                        animatorController.Parameters.AddFloat(conditionsAnd.Parameter.Name);
                        float value = conditionsAnd.Parameter.Value;
                        prevValue = value;

                        conditionTree.Append(fallbackTree, value - Epsilon);
                        conditionTree.Append(fallbackTree, value + Epsilon);
                        nextTree = conditionTree;
                    }

                    if (prevValue is { } x)
                    {
                        nextTree?.AddMotion(expressionMotion, x);
                    }

                    nextTree = fallbackTree;
                }
            }

            nextTree?.AddMotion(defM);
        }

        var idleState = stateMachine.AddState("DBT (DO NOT OPEN IN EDITOR!) (WD On)");
        idleState.Motion = blendTree.Build();

    }

    private sealed class ConditionExpressionComparer : IEqualityComparer<IModEmoExpression>
    {
        public static ConditionExpressionComparer Instance { get; } = new();
        public bool Equals(IModEmoExpression x, IModEmoExpression y)
            => GetHashCode(x) == GetHashCode(y);

        public int GetHashCode(IModEmoExpression obj)
        {
            HashCode hash = new();
            foreach(var x in obj.Conditions)
            {
                foreach(var y in x)
                {
                    hash.Add(y);
                }
            }
            return hash.ToHashCode();
        }
    }

    private static void ApplyTransitionWithCondition(StateBuilder state, AnimatorParameterCondition condition, Func<StateBuilder, TransitionBuilder> transitionBuilder)
    {
        var mode = condition.Mode;
        var name = condition.Parameter.Name;
        switch (condition.Parameter.Value.Type)
        {
            case AnimatorControllerParameterType.Bool:
                {
                    var value = (bool)condition.Parameter.Value;
                    if (mode is ConditionMode.Equals)
                    {
                        transitionBuilder(state).AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, name, 0);
                    }
                    else
                    {
                        transitionBuilder(state).AddCondition(value ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, name, 0);
                    }
                }
                break;
            case AnimatorControllerParameterType.Int:
                {
                    if (mode.HasFlag(ConditionMode.Equals))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Equals, name, condition.Parameter.Value);
                    }
                    else if (mode.HasFlag(ConditionMode.NotEqual))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.NotEqual, name, condition.Parameter.Value);
                    }
                    if (mode.HasFlag(ConditionMode.GreaterThan))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value);
                    }
                    else if (mode.HasFlag(ConditionMode.LessThan))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value);
                    }
                }
                break;
            case AnimatorControllerParameterType.Float:
                {
                    if (mode.HasFlag(ConditionMode.Equals))
                    {
                        transitionBuilder(state)
                            .AddCondition(AnimatorConditionMode.Less, name, condition.Parameter.Value + float.Epsilon)
                            .AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value - float.Epsilon);
                    }
                    else if (mode.HasFlag(ConditionMode.NotEqual))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Less, name, condition.Parameter.Value - float.Epsilon);
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value + float.Epsilon);
                    }
                    if (mode.HasFlag(ConditionMode.GreaterThan))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value);
                    }
                    else if (mode.HasFlag(ConditionMode.LessThan))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value);
                    }
                }
                break;
        }
    }

    public static VirtualLayer? GenerateBlinkController(BuildContext context)
    {
        var vcc = context.GetVirtualControllerContext();
        var modEmo = context.GetModEmoContext().Root;
        if (modEmo.Settings.BlinkExpression == null)
            return default;

        var layer = VirtualLayer.Create(vcc.CloneContext, "[ModEmo] Blink");
        var tree = new DirectBlendTree();

        var data = context.GetData();
        data.Parameters.Add(new($"{ParameterNames.Blink.Enable}", 1f));
        data.Parameters.Add(new($"{ParameterNames.Blink.Disable}", 0f));
        data.Parameters.Add(new($"{ParameterNames.Blink.Result}", 0f));

        // Gate
        {
            var result_0 = AnimationUtils.CreateAAPClip(ParameterNames.Blink.Result, 0);
            var result_1 = AnimationUtils.CreateAAPClip(ParameterNames.Blink.Result, 1);

            var t = tree.AddBlendTree("Gate");
            t.BlendParameter = ParameterNames.Blink.Enable;
            t.AddMotion(result_0);
            t = t.AddBlendTree("Gate 2");
            t.BlendParameter = ParameterNames.Blink.Disable;
            t.AddMotion(result_1);
            t.AddMotion(result_0);
        }

        // Motion
        {
            var clip = MakeAnimationClip(modEmo.Settings.BlinkExpression);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var t = tree.AddBlendTree("Motion");
            t.BlendParameter = ParameterNames.Blink.Result;
            t.AddMotion(data.BlankClip, 0);
            t.AddMotion(data.BlankClip, 0.9999f);
            t.AddMotion(clip, 1);
        }

        static AnimationClip MakeAnimationClip(IModEmoExpression expression)
        {
            var clip = new AnimationClip() { name = expression.Name };

            var frames = expression.Frames.GroupBy(x => (x.Keyframe, Frame: x), x => x.BlendShapes, (x, y) => new ExpressionFrame(x.Keyframe, y.SelectMany(y => y), x.Frame.Publisher), new Comparer()).ToArray();

            List<KeyValuePair<EditorCurveBinding, Keyframe>> aaa = new();

            foreach (var frame in frames)
            {
                var (key, blendshapes) = frame;
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

        layer.StateMachine!.AddState("DirectBlendTree (WD On)", vcc.Clone(tree.Build(context.AssetContainer)));

        return layer;
    }

    private sealed class Comparer : IEqualityComparer<(float Keyframe, IModEmoExpressionFrame Frame)>
    {
        public bool Equals((float Keyframe, IModEmoExpressionFrame Frame) x, (float Keyframe, IModEmoExpressionFrame Frame) y)
            => x.Keyframe == y.Keyframe;

        public int GetHashCode((float Keyframe, IModEmoExpressionFrame Frame) obj)
            => obj.Keyframe.GetHashCode();
    }

    private struct AnimationData
    {
        public AnimationData(AnimationClip clip, string? timeParameterName = null)
        {
            Clip = clip;
            TimeParameterName = timeParameterName;
        }

        public AnimationClip? Clip;
        public string? TimeParameterName;

        public sealed class EqualityComparer : IEqualityComparer<AnimationData>
        {
            public static EqualityComparer Default => new();

            bool IEqualityComparer<AnimationData>.Equals(AnimationData x, AnimationData y) => x.Clip == y.Clip;

            int IEqualityComparer<AnimationData>.GetHashCode(AnimationData obj) => obj.Clip!.GetHashCode();
        }
    }
}