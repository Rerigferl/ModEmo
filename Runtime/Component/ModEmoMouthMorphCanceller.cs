namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Canceller")]
    internal sealed class ModEmoMouthMorphCanceller : ModEmoTagComponent, IModEmoMouthMorphCanceller
    {
        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach(var x in this.GetOwnedComponents())
            {
                foreach (var y in x.GetUsedBlendshapeNames())
                {
                    hashCode.Add(y);
                }
            }
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