namespace Numeira;

internal static partial class ModEmoConstants
{
    public abstract class Parameters
    {
        public const string Prefix = "ME/";

        public static class BlendShapes
        {
            public const string Prefix = $"{Parameters.Prefix}{nameof(BlendShapes)}/";
        }

        public static class Internal
        {
            public const string Prefix = $"{Parameters.Prefix}{nameof(Internal)}/";

            public const string One = $"{Prefix}One";
            public const string ExpressionIndex = $"{Prefix}ExpressionIndex";
            public const string ExpressionIndexSmoothed = $"{Prefix}ExpressionIndexSmoothed";

            public static class BlendShapes
            {
                public const string Prefix = $"{Internal.Prefix}{nameof(BlendShapes)}/";

                public const string ControlPrefix = $"{Prefix}Control/";
                public const string OverridePrefix = $"{Prefix}Override/";
                public const string DisablePrefix = $"{Prefix}Disable/";
            }

            public static class Input
            {
                public const string Prefix = $"{Internal.Prefix}{nameof(Input)}/";

                public const string Left = $"{Prefix}Left";
                public const string Right = $"{Prefix}Right";
            }
        }
    }
}