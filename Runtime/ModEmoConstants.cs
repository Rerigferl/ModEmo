﻿namespace Numeira;

internal static partial class ModEmoConstants
{
    public static partial class Parameters
    {
        public const string Prefix = "ME/";

        public static class Expression
        {
            public const string Prefix = $"{Parameters.Prefix}{nameof(Expression)}/";
            public const string Pattern = $"{Prefix}Pattern";
            public const string Index = $"{Prefix}Index";
            public const string Lock = $"{Prefix}Lock";
        }

        public static class BlendShapes
        {
            public const string Prefix = $"{Parameters.Prefix}{nameof(BlendShapes)}/";
        }

        public static class Internal
        {
            public const string Prefix = $"{Parameters.Prefix}I/";

            public const string One = $"{Prefix}One";
            public const string ExpressionIndex = $"{Prefix}ExpressionIndex";
            public const string ExpressionIndexSmoothed = $"{Prefix}ExpressionIndexSmoothed";

            public static class Expression
            {
                public const string Prefix = $"{Internal.Prefix}{nameof(Expression)}/";

                public const string Index = $"{Prefix}Index";
            }

            public static class BlendShapes
            {
                public const string Prefix = $"{Internal.Prefix}{nameof(BlendShapes)}/";

                public const string ControlPrefix = $"{Prefix}C/";
                public const string OverridePrefix = $"{Prefix}O/";
                public const string DisablePrefix = $"{Prefix}D/";
            }

            public static class Input
            {
                public const string Prefix = $"{Internal.Prefix}{nameof(Input)}/";

                public const string Left = $"{Prefix}Left";
                public const string Right = $"{Prefix}Right";
                public const string Switch = $"{Prefix}Switch";
                public const string Override = $"{Prefix}Override";
            }
        }
    }
}