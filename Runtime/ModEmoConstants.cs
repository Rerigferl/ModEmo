namespace Numeira;

internal static partial class ModEmoConstants
{
    public abstract class Parameters
    {
        public const string Prefix = "ModEmo/";

        public static class Internal
        {
            public const string Prefix = $"{Parameters.Prefix}{nameof(Internal)}/";

            public const string One = $"{Prefix}One";

            public static class BlendShapes
            {
                public const string Prefix = $"{Internal.Prefix}{nameof(BlendShapes)}/";

                public const string ControlPrefix = $"{Prefix}Control/";
                public const string OverridePrefix = $"{Prefix}Override/";
                public const string DisablePrefix = $"{Prefix}Disable/";
            }
        }
    }
}