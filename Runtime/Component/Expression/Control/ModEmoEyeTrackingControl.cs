namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    [AddComponentMenu(ComponentMenuPrefix + "EyeTracking Control")]
    internal sealed class ModEmoEyeTrackingControl : ModEmoTagComponent, IModEmoEyeTrackingControl
    {
        private void OnEnable() { }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(enabled);
        }
    }

    internal interface IModEmoEyeTrackingControl : IModEmoExpressionControl
    {

    }
}