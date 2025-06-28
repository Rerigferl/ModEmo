
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShapes Selector")]
    internal sealed class ModEmoBlendShapesSelector : ModEmoTagComponent, IModEmoExpressionFrame
    {
        public BlendShape[] BlendShapes = { };

        public IModEmoComponent? Publisher => this;

        float IModEmoExpressionFrame.Keyframe => 0;

        IEnumerable<BlendShape> IModEmoExpressionFrame.BlendShapes => BlendShapes;
    }
}