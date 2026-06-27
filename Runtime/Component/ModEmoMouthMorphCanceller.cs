namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Canceller")]
    internal sealed class ModEmoMouthMorphCanceller : ModEmoTagComponent, IModEmoMouthMorphCanceller
    {
        public IModEmoBlendshapeConsumer[] Children => this.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(true);

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in Children)
            {
                x.CalculateContentHash(ref hashCode);
            }
        }

        public IEnumerable<BlendShape> GetUsedBlendshapes() => this.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(includeSelf: true).SelectMany(x => x.GetUsedBlendshapes());

    }

    internal interface IModEmoMouthMorphCanceller : IModEmoBlendshapeConsumer
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