
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Canceller")]
    internal sealed class ModEmoMouthMorphCanceller : ModEmoExpressionFrameFolder, IModEmoMouthMorphCanceller
    {
        protected override bool IncludeSelf => true;
    }

    internal interface IModEmoMouthMorphCanceller : IModEmoExpressionFrameProvider
    { }
}