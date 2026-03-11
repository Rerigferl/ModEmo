namespace Numeira
{
    [AddComponentMenu($"{ComponentMenuPrefix}Runtime Blendshape Controller")]
    internal sealed class ModEmoRuntimeBlendshapeController : ModEmoTagComponent, IModEmoRuntimeBlendshapeController
    {
        protected override void CalculateContentHash(ref HashCode hashCode)
        {
        }

        public bool Sync = false;
        public string[] Blacklist = { };

        bool IModEmoRuntimeBlendshapeController.Sync => Sync;
        IEnumerable<string> IModEmoRuntimeBlendshapeController.Blacklist => Blacklist;
    }

    internal interface IModEmoRuntimeBlendshapeController : IModEmoComponent
    {
        public IEnumerable<string> Blacklist { get; }
        public bool Sync { get; }
    }
}
