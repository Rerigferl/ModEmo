namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    [ExecuteInEditMode]
    internal class ModEmoDefaultExpression : ModEmoExpression
    {
        public ExpressionMode Mode;
        protected override ExpressionMode GetMode() => Mode;
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(ModEmoExpressionSettings))]
    internal abstract class ModEmoExpression : ModEmoTagComponent, IModEmoExpression
    {
        public string Name = "";
        public bool DesyncWithObjectName = false;

        public ModEmoExpressionSettings Settings => gameObject.GetOrAddComponent<ModEmoExpressionSettings>();

        string IModEmoExpression.Name => GetName();

        ExpressionMode IModEmoExpression.Mode => GetMode();

        IEnumerable<IModEmoExpressionFrame> IModEmoExpression.Frames => GetFrames();

        bool IModEmoExpression.Loop => IsLoop();

        protected virtual string GetName() => DesyncWithObjectName ? Name : name;
        protected virtual ExpressionMode GetMode() => ExpressionMode.Default;
        protected virtual IEnumerable<IModEmoExpressionFrame> GetFrames()
        {
            if (Settings.MotionFolder is { } motionFolder)
            {
                foreach (var frame in motionFolder.gameObject.GetComponentsInDirectChildren<IModEmoExpressionFrame>())
                {
                    yield return frame;
                }
            }
        }
        protected virtual bool IsLoop() => GetComponent<IModEmoExpressionLoopControl>() != null;
    }

    internal interface IModEmoExpression : IModEmoComponent
    {
        string Name { get; }

        ExpressionMode Mode { get; }

        IEnumerable<IModEmoExpressionFrame> Frames { get; }

        ModEmoExpressionSettings Settings { get; }

        bool Loop { get; }
    }

    internal enum ExpressionMode
    {
        Default,
        Combine,
    }
}