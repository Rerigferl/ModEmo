namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Canceller")]
    internal sealed class ModEmoMouthMorphCanceller : ModEmoTagComponent, IModEmoMouthMorphCanceller
    {
        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            // TODO!
        }

    }

    internal interface IModEmoMouthMorphCanceller : IOwnerComponent<IBlendshapeWriterComponent>
    { }

#if UNITY_EDITOR
    static partial class RuntimeEditor
    {
        [CustomEditor(typeof(ModEmoMouthMorphCanceller))]
        public sealed class ModEmoMouthMorphCancellerEditor : Editor
        {

        }
    }
#endif
}