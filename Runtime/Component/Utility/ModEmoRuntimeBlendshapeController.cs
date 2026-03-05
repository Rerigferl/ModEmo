namespace Numeira
{
    internal sealed class ModEmoRuntimeBlendshapeController : ModEmoTagComponent, IModEmoRuntimeBlendshapeController
    {
        protected override void CalculateContentHash(ref HashCode hashCode)
        {
        }

        public string[] Blacklist = { };

        IEnumerable<string> IModEmoRuntimeBlendshapeController.Blacklist => Blacklist;
    }

    internal interface IModEmoRuntimeBlendshapeController : IModEmoComponent
    {
        public IEnumerable<string> Blacklist { get; }
    }
}
