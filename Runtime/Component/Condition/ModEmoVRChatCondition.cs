namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Gesture Condition")]
    internal sealed class ModEmoVRChatCondition : ModEmoConditionBase
    {
        public Hand Hand = Hand.Left;
        public Gesture Gesture = Gesture.Fist;

        public override IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            yield return Group.Create(this, Factory);

            static IEnumerable<AnimatorParameterCondition> Factory(ModEmoVRChatCondition x)
            {
                if (x.Hand.HasFlag(Hand.Left))
                {
                    yield return new AnimatorParameterCondition(new AnimatorParameter("GestureLeft", (int)x.Gesture), ConditionMode.Equals);
                }
                if (x.Hand.HasFlag(Hand.Right))
                {
                    yield return new AnimatorParameterCondition(new AnimatorParameter("GestureRight", (int)x.Gesture), ConditionMode.Equals);
                }
            }
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(Hand);
            hashCode.Add(Gesture);
        }
    }
}