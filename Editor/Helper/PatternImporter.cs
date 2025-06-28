namespace Numeira;

internal static class PatternImporter
{
    [MenuItem("Tests/Import animator")]
    public static void Test()
    {
        var ac = Selection.activeObject as AnimatorController;
        if (ac != null )
            ImportFromVRChatFX( ac );
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
        patternObj.AddComponent<ModEmoExpressionPatterns>();

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

            exp.Settings.ConditionFolder ??= ModEmoExpressionConditionFolder.New(exp.transform);

            foreach(var bit in bits)
            {
                if (!maskPatterns.TryGetValue(bit, out var maskPattern))
                    continue;

                var conditionObj = new GameObject();
                conditionObj.transform.parent = exp.Settings.ConditionFolder.transform;
                var condition = conditionObj.AddComponent<ModEmoVRChatCondition>();

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

    private sealed class MaskGroupComparer : IComparer<uint>
    {
        public static readonly MaskGroupComparer Instance = new MaskGroupComparer();

        public int Compare(uint x, uint y)
        {
            static bool IsRight(uint a) => (a & 0x1100) != 0;

            return (IsRight(x), IsRight(y)) switch
            {
                (false, false) or (true, true) => 0,
                (false, true) => -1,
                (true, false) => 1,
            };
        }
    }
}
