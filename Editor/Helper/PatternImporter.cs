namespace Numeira;

internal static partial class PatternImporter
{
    public static GameObject ImportFromAnimatorController(AnimatorController animatorController)
    {
        var layers = animatorController.layers;

        Dictionary<AnimatorState, Vector3> positions = new();
        Dictionary<AnimatorState, int> index = new();
        Dictionary<AnimatorState, List<AnimatorStateTransition>> dict = new();

        foreach (var layer in animatorController.layers)
        {
            var stateMachine = layer.stateMachine;
            foreach(var (state, position) in stateMachine.states.OrderBy(x => Vector3.Magnitude(x.position)))
            {
                positions.TryAdd(state, position);
                foreach(var transition in state.transitions)
                {
                    if (transition.destinationState is { } dest)
                    {
                        index.TryAdd(dest, index.Count);
                        dict.GetOrAdd(dest, _ => new()).Add(transition);
                    }
                }
            }

            foreach (var transition in stateMachine.anyStateTransitions.OrderBy(x => x.destinationState == null ? 0 : Vector3.Magnitude(positions[x.destinationState])))
            {
                if (transition.destinationState is { } dest)
                {
                    index.TryAdd(dest, index.Count);
                    dict.GetOrAdd(dest, _ => new()).Add(transition);
                }
            }
        }

        var patternObj = new GameObject(animatorController.name);
        patternObj.AddComponent<ModEmoExpressionPattern>();

        foreach(var (state, transitions) in dict.OrderBy(x => index[x.Key]))
        {
            var motion = state.motion;
            bool isFacialAnimation = false;
            if (motion is not AnimationClip anim)
                continue;

            var bindings = AnimationUtility.GetCurveBindings(anim);
            foreach (var binding in bindings)
            {
                var path = binding.path;
                if (path is not "Body")
                    continue;
                isFacialAnimation = true;
                break;
            }

            if (!isFacialAnimation)
                continue;

            var expressionObj = new GameObject(anim.name);
            expressionObj.transform.parent = patternObj.transform;
            var expression = expressionObj.AddComponent<ModEmoAnimationClipExpression>();
            expression.AnimationClip = anim;

            foreach(var (transition, idx) in transitions.Index())
            {
                var conditions = transition.conditions;
                if (conditions.Length == 0)
                    continue;

                if (conditions.Length == 1)
                {
                    var c = expressionObj.AddComponent<ModEmoCondition>();
                    c.Parameters.Add(new() { Parameter = new(conditions[0].parameter, new(conditions[0].threshold)), Mode = ConditionMode.Equals });
                }
                else
                {
                    var conditionObj = new GameObject($"Condition ({idx + 1})");
                    conditionObj.transform.parent = expressionObj.transform;
                    var f = conditionObj.AddComponent<ModEmoConditionFolder>();
                    foreach(var condition in conditions)
                    {
                        var c = new GameObject(condition.parameter);
                        c.transform.parent = conditionObj.transform;
                        c.AddComponent<ModEmoCondition>().Parameters.Add(new() { Parameter = new(condition.parameter, new(condition.threshold)), Mode = ConditionMode.Equals });
                    }
                }
            }
        }

        return patternObj;
    }

    public static GameObject? ImportFromVRChatFX(AnimatorController animatorController)
    {
        // TODO: ANDはできてるけどORはできてない

        var layers = animatorController.layers;

        Dictionary<uint, (int Index, int Side, AnimatorState State)> dict = new();

        for (int i = 3; i >= 0; i--)
        {
            var layer = layers[i];

            var stateMachine = layer.stateMachine;
            if (stateMachine == null)
                continue;

            foreach (var transition in stateMachine.anyStateTransitions)
            {
                if (transition.destinationState is not { } state || state.motion is not AnimationClip clip)
                    continue;

                uint mask = 0u;
                foreach(var cond in transition.conditions)
                {
                    if (cond.parameter is not ("GestureLeft" or "GestureRight") || cond.threshold is not (>= 0 and < 8))
                        continue;

                    var a = cond.parameter is "GestureLeft" ? 0x001u : 0x100u;
                    a <<= (int)cond.threshold;
                    mask |= a;
                }

                if (mask == 0)
                    continue;

                dict[mask] = (transition.conditions.Select(x => (int)x.threshold).Max(), i, state);
            }
        }

        if (dict.Count == 0)
            return default;

        var patternObj = new GameObject(animatorController.name);
        patternObj.AddComponent<ModEmoExpressionPattern>();

        var span = (stackalloc ushort[16 /* sizeof(ushort) * 8 */]);

        var gestures = new Gesture?[] { null }.Concat(Enum.GetValues(typeof(Gesture)).Cast<Gesture?>());
        var maskPatterns = gestures.SelectMany(x => gestures.Select(y => (Left: x, Right: y)))
            .Select(x =>
            {
                return (Mask: GestureToMask(x.Left, x.Right), Left: x.Left, Right: x.Right);
                static uint GestureToMask(Gesture? left, Gesture? right) => ((right is null ? 0 : 0b_0000_0001_0000_0000u << (int)right.Value) | (left is null ? 0 : (0b_0000_0000_0000_0001u << (int)left)));
            })
            .ToDictionary(x => x.Mask, x => (x.Left, x.Right));

        foreach (var (key, item) in dict.OrderByDescending(x => x.Value.Side).ThenBy(x => x.Value.Index))
        {
            var bits = DeconstructPopBits((ushort)key, span);
            if (bits.Length == 0) 
            { 
                continue;
            }

            var expObj = new GameObject(item.State.name);
            expObj.transform.parent = patternObj.transform;
            var exp = expObj.AddComponent<ModEmoAnimationClipExpression>();
            exp.AnimationClip = item.State.motion as AnimationClip;

            //exp.Settings.ConditionFolder ??= ModEmoConditionFolder.New(exp.transform);

            foreach(var bit in bits)
            {
                if (!maskPatterns.TryGetValue(bit, out var maskPattern))
                    continue;

                var conditionObj = new GameObject();
                //conditionObj.transform.parent = exp.Settings.ConditionFolder.transform;
                var condition = conditionObj.AddComponent<ModEmoGestureCondition>();

                if (maskPattern.Left != null)
                {
                    condition.Hand = Hand.Left;
                    condition.Gesture = maskPattern.Left.Value;
                }
                else if (maskPattern.Right != null)
                {
                    condition.Hand = Hand.Right;
                    condition.Gesture = maskPattern.Right.Value;
                }
                condition.gameObject.name = $"{condition.Gesture} ({condition.Hand})";
            }
        }

        static ReadOnlySpan<ushort> DeconstructPopBits(ushort mask, Span<ushort> span)
        {
            Debug.Assert(span.Length >= 16);
            int count = 0;
            for (int i = 0; i < 16; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    span[count++] = (ushort)(1u << i);
                }
            }
            return span[..count];
        }

        return patternObj;
    }

    public static GameObject ImportFromFaceEmo()
    {
        throw new NotImplementedException();
    }
}
