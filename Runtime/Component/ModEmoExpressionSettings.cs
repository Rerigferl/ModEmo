namespace Numeira
{
    [AddComponentMenu($"/{ComponentMenuPrefix}Expression Settings")]
    [ExecuteInEditMode]
    internal sealed class ModEmoExpressionSettings : ModEmoTagComponent, IModEmoExpressionSettings
    {
        public ModEmoExpressionConditionFolder? ConditionFolder;
        public ModEmoExpressionMotionFolder? MotionFolder;

#if UNITY_EDITOR
        private void _UnityEvents()
        {
            OnEnable();
            OnDisable();
        }

        private void OnEnable()
        {
#if !MODEMO_DEBUG
            hideFlags |= HideFlags.HideInInspector | HideFlags.HideInHierarchy;
#else
            hideFlags &= ~HideFlags.HideInInspector;
#endif
        }

        private void OnDisable()
        {

        }
#endif
    }

    internal interface IModEmoExpressionSettings : IModEmoComponent
    {
    }
}