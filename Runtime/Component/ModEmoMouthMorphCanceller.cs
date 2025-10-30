
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Canceller")]
    internal sealed class ModEmoMouthMorphCanceller : ModEmoBlendShapeFolder, IModEmoMouthMorphCanceller
    {
        protected override bool IncludeSelf => true;
    }

    internal interface IModEmoMouthMorphCanceller : IModEmoBlendShapeProvider
    { }
}