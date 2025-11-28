namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Canceller")]
    internal sealed class ModEmoMouthMorphCanceller : ModEmoTagComponent, IModEmoMouthMorphCanceller
    {
        public IModEmoBlendShapeProvider[] Children => this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(true);

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in Children)
            {
                x.CalculateContentHash(ref hashCode);
            }
        }

        public IEnumerable<BlendShape> GetBlendShapes() => this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: true).SelectMany(x => x.GetBlendShapes());

    }

    internal interface IModEmoMouthMorphCanceller : IModEmoBlendShapeProvider
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