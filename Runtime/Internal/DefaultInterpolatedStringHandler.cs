// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file was originally part of the .NET runtime repository:
// https://github.com/dotnet/runtime/blob/e231b754d3aa5da17571dc476c5a5f599841b5c6/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/DefaultInterpolatedStringHandler.cs

using System.Buffers;
using System.Globalization;

namespace System.Runtime.CompilerServices;

[InterpolatedStringHandler]
internal ref struct DefaultInterpolatedStringHandler
{
    /// <summary>Expected average length of formatted data used for an individual interpolation expression result.</summary>
    /// <remarks>
    /// This is inherited from string.Format, and could be changed based on further data.
    /// string.Format actually uses `format.Length + args.Length * 8`, but format.Length
    /// includes the format items themselves, e.g. "{0}", and since it's rare to have double-digit
    /// numbers of items, we bump the 8 up to 11 to account for the three extra characters in "{d}",
    /// since the compiler-provided base length won't include the equivalent character count.
    /// </remarks>
    private const int GuessedLengthPerHole = 11;
    /// <summary>Minimum size array to rent from the pool.</summary>
    /// <remarks>Same as stack-allocation size used today by string.Format.</remarks>
    private const int MinimumArrayPoolLength = 256;

    /// <summary>Optional provider to pass to IFormattable.ToString or ISpanFormattable.TryFormat calls.</summary>
    private readonly IFormatProvider? _provider;
    /// <summary>Array rented from the array pool and used to back <see cref="_chars"/>.</summary>
    private char[]? _arrayToReturnToPool;
    /// <summary>The span to write into.</summary>
    private Span<char> _chars;
    /// <summary>Position at which to write the next character.</summary>
    private int _pos;
    /// <summary>Whether <see cref="_provider"/> provides an ICustomFormatter.</summary>
    /// <remarks>
    /// Custom formatters are very rare.  We want to support them, but it's ok if we make them more expensive
    /// in order to make them as pay-for-play as possible.  So, we avoid adding another reference type field
    /// to reduce the size of the handler and to reduce required zero'ing, by only storing whether the provider
    /// provides a formatter, rather than actually storing the formatter.  This in turn means, if there is a
    /// formatter, we pay for the extra interface call on each AppendFormatted that needs it.
    /// </remarks>
    private readonly bool _hasCustomFormatter;

    /// <summary>Creates a handler used to translate an interpolated string into a <see cref="string"/>.</summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
    public DefaultInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _provider = null;
        _chars = _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount));
        _pos = 0;
        _hasCustomFormatter = false;
    }

    /// <summary>Creates a handler used to translate an interpolated string into a <see cref="string"/>.</summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
    public DefaultInterpolatedStringHandler(int literalLength, int formattedCount, IFormatProvider? provider)
    {
        _provider = provider;
        _chars = _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount));
        _pos = 0;
        _hasCustomFormatter = provider is not null && HasCustomFormatter(provider);
    }

    /// <summary>Creates a handler used to translate an interpolated string into a <see cref="string"/>.</summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="initialBuffer">A buffer temporarily transferred to the handler for use as part of its formatting.  Contents may be overwritten.</param>
    /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
    public DefaultInterpolatedStringHandler(int literalLength, int formattedCount, IFormatProvider? provider, Span<char> initialBuffer)
    {
        _provider = provider;
        _chars = initialBuffer;
        _arrayToReturnToPool = null;
        _pos = 0;
        _hasCustomFormatter = provider is not null && HasCustomFormatter(provider);
    }

    /// <summary>Derives a default length with which to seed the handler.</summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetDefaultLength(int literalLength, int formattedCount) =>
        Math.Max(MinimumArrayPoolLength, literalLength + (formattedCount * GuessedLengthPerHole));

    /// <summary>Gets the built <see cref="string"/>.</summary>
    /// <returns>The built string.</returns>
    public override string ToString() => new string(Text);

    /// <summary>Gets the built <see cref="string"/> and clears the handler.</summary>
    /// <returns>The built string.</returns>
    /// <remarks>
    /// This releases any resources used by the handler. The method should be invoked only
    /// once and as the last thing performed on the handler. Subsequent use is erroneous, ill-defined,
    /// and may destabilize the process, as may using any other copies of the handler after
    /// <see cref="ToStringAndClear" /> is called on any one of them.
    /// </remarks>
    public string ToStringAndClear()
    {
        string result = new string(Text);
        Clear();
        return result;
    }

    /// <summary>Clears the handler.</summary>
    /// <remarks>
    /// This releases any resources used by the handler. The method should be invoked only
    /// once and as the last thing performed on the handler. Subsequent use is erroneous, ill-defined,
    /// and may destabilize the process, as may using any other copies of the handler after <see cref="Clear"/>
    /// is called on any one of them.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        char[]? toReturn = _arrayToReturnToPool;

        _arrayToReturnToPool = null;
        _chars = default;
        _pos = 0;

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    /// <summary>Gets a span of the characters appended to the handler.</summary>
    public ReadOnlySpan<char> Text => _chars.Slice(0, _pos);

    /// <summary>Writes the specified string to the handler.</summary>
    /// <param name="value">The string to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLiteral(string value)
    {
        if (value.AsSpan().TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            GrowThenCopyString(value);
        }
    }

    #region AppendFormatted
    //
    //
    //
    //
    //

    #region AppendFormatted T
    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    public void AppendFormatted<T>(T value)
    {
        if (_hasCustomFormatter)
        {
            AppendCustomFormatter(value, format: null);
            return;
        }

        if (SpanFormatter<T>.IsFormattable)
        {
            int charsWritten;
            while (!SpanFormatter<T>.TryFormat(value, _chars.Slice(_pos), out charsWritten, default, _provider))
            {
                Grow();
            }

            _pos += charsWritten;
            return;
        }

        string? s;
        if (value is IFormattable)
        {
            s = ((IFormattable)value).ToString(format: null, _provider);
        }
        else
        {
            s = value?.ToString();
        }

        if (s is not null)
        {
            AppendLiteral(s);
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format string.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    public void AppendFormatted<T>(T value, string? format)
    {
        if (_hasCustomFormatter)
        {
            AppendCustomFormatter(value, format);
            return;
        }

        if (SpanFormatter<T>.IsFormattable)
        {
            int charsWritten;
            while (!SpanFormatter<T>.TryFormat(value, _chars.Slice(_pos), out charsWritten, default, _provider))
            {
                Grow();
            }

            _pos += charsWritten;
            return;
        }

        string? s;
        if (value is IFormattable)
        {
            s = ((IFormattable)value).ToString(format, _provider);
        }
        else
        {
            s = value?.ToString();
        }

        if (s is not null)
        {
            AppendLiteral(s);
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    public void AppendFormatted<T>(T value, int alignment)
    {
        int startingPos = _pos;
        AppendFormatted(value);
        if (alignment != 0)
        {
            AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format string.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        int startingPos = _pos;
        AppendFormatted(value, format);
        if (alignment != 0)
        {
            AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
        }
    }
    #endregion

    #region AppendFormatted ReadOnlySpan<char>
    /// <summary>Writes the specified character span to the handler.</summary>
    /// <param name="value">The span to write.</param>
    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        if (value.TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            GrowThenCopySpan(value);
        }
    }

    /// <summary>Writes the specified string of chars to the handler.</summary>
    /// <param name="value">The span to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
    {
        bool leftAlign = false;
        if (alignment < 0)
        {
            leftAlign = true;
            alignment = -alignment;
        }

        int paddingRequired = alignment - value.Length;
        if (paddingRequired <= 0)
        {
            AppendFormatted(value);
            return;
        }

        EnsureCapacityForAdditionalChars(value.Length + paddingRequired);
        if (leftAlign)
        {
            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
            _chars.Slice(_pos, paddingRequired).Fill(' ');
            _pos += paddingRequired;
        }
        else
        {
            _chars.Slice(_pos, paddingRequired).Fill(' ');
            _pos += paddingRequired;
            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }
    }
    #endregion

    #region AppendFormatted string
    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    public void AppendFormatted(string? value)
    {
        if (!_hasCustomFormatter &&
            value is not null &&
            value.AsSpan().TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            AppendFormattedSlow(value);
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <remarks>
    /// Slow path to handle a custom formatter, potentially null value,
    /// or a string that doesn't fit in the current buffer.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendFormattedSlow(string? value)
    {
        if (_hasCustomFormatter)
        {
            AppendCustomFormatter(value, format: null);
        }
        else if (value is not null)
        {
            EnsureCapacityForAdditionalChars(value.Length);
            value.AsSpan().CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    public void AppendFormatted(string? value, int alignment = 0, string? format = null) =>
        AppendFormatted<string?>(value, alignment, format);
    #endregion

    #region AppendFormatted object
    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    public void AppendFormatted(object? value, int alignment = 0, string? format = null) =>
        AppendFormatted<object?>(value, alignment, format);
    #endregion
    #endregion

    /// <summary>Gets whether the provider provides a custom formatter.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasCustomFormatter(IFormatProvider provider)
    {
        Debug.Assert(provider is not null);
        Debug.Assert(provider is not CultureInfo || provider.GetFormat(typeof(ICustomFormatter)) is null, "Expected CultureInfo to not provide a custom formatter");
        return
            provider!.GetType() != typeof(CultureInfo) &&
            provider.GetFormat(typeof(ICustomFormatter)) != null;
    }

    /// <summary>Formats the value using the custom formatter from the provider.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format string.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendCustomFormatter<T>(T value, string? format)
    {
        Debug.Assert(_hasCustomFormatter);
        Debug.Assert(_provider != null);

        ICustomFormatter? formatter = (ICustomFormatter?)_provider!.GetFormat(typeof(ICustomFormatter));
        Debug.Assert(formatter != null, "An incorrectly written provider said it implemented ICustomFormatter, and then didn't");

        if (formatter is not null && formatter.Format(format, value, _provider) is string customFormatted)
        {
            AppendLiteral(customFormatted);
        }
    }

    /// <summary>Handles adding any padding required for aligning a formatted value in an interpolation expression.</summary>
    /// <param name="startingPos">The position at which the written value started.</param>
    /// <param name="alignment">Non-zero minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    private void AppendOrInsertAlignmentIfNeeded(int startingPos, int alignment)
    {
        Debug.Assert(startingPos >= 0 && startingPos <= _pos);
        Debug.Assert(alignment != 0);

        int charsWritten = _pos - startingPos;

        bool leftAlign = false;
        if (alignment < 0)
        {
            leftAlign = true;
            alignment = -alignment;
        }

        int paddingNeeded = alignment - charsWritten;
        if (paddingNeeded > 0)
        {
            EnsureCapacityForAdditionalChars(paddingNeeded);

            if (leftAlign)
            {
                _chars.Slice(_pos, paddingNeeded).Fill(' ');
            }
            else
            {
                _chars.Slice(startingPos, charsWritten).CopyTo(_chars.Slice(startingPos + paddingNeeded));
                _chars.Slice(startingPos, paddingNeeded).Fill(' ');
            }

            _pos += paddingNeeded;
        }
    }

    /// <summary>Ensures <see cref="_chars"/> has the capacity to store <paramref name="additionalChars"/> beyond <see cref="_pos"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacityForAdditionalChars(int additionalChars)
    {
        if (_chars.Length - _pos < additionalChars)
        {
            Grow(additionalChars);
        }
    }

    /// <summary>Fallback for fast path in <see cref="AppendLiteral(string)"/> when there's not enough space in the destination.</summary>
    /// <param name="value">The string to write.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopyString(string value)
    {
        Grow(value.Length);
        value.AsSpan().CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    /// <summary>Fallback for <see cref="AppendFormatted(ReadOnlySpan{char})"/> for when not enough space exists in the current buffer.</summary>
    /// <param name="value">The span to write.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopySpan(ReadOnlySpan<char> value)
    {
        Grow(value.Length);
        value.CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    /// <summary>Grows <see cref="_chars"/> to have the capacity to store at least <paramref name="additionalChars"/> beyond <see cref="_pos"/>.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalChars)
    {
        Debug.Assert(additionalChars > _chars.Length - _pos);
        GrowCore((uint)_pos + (uint)additionalChars);
    }

    /// <summary>Grows the size of <see cref="_chars"/>.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        GrowCore((uint)_chars.Length + 1);
    }

    /// <summary>Grow the size of <see cref="_chars"/> to at least the specified <paramref name="requiredMinCapacity"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GrowCore(uint requiredMinCapacity)
    {

        uint newCapacity = Math.Max(requiredMinCapacity, Math.Min((uint)_chars.Length * 2, 0x3FFFFFDF));
        int arraySize = (int)Math.Clamp(newCapacity, MinimumArrayPoolLength, int.MaxValue);

        char[] newArray = ArrayPool<char>.Shared.Rent(arraySize);
        _chars.Slice(0, _pos).CopyTo(newArray);

        char[]? toReturn = _arrayToReturnToPool;
        _chars = _arrayToReturnToPool = newArray;

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    internal static class SpanFormatter<T>
    {
        public static bool IsFormattable =>
            typeof(T) == typeof(byte)           ||
            typeof(T) == typeof(sbyte)          ||
            typeof(T) == typeof(char)           ||
            typeof(T) == typeof(short)          ||
            typeof(T) == typeof(ushort)         ||
            typeof(T) == typeof(int)            ||
            typeof(T) == typeof(uint)           ||
            typeof(T) == typeof(long)           ||
            typeof(T) == typeof(ulong)          ||
            typeof(T) == typeof(float)          ||
            typeof(T) == typeof(double)         ||
            typeof(T) == typeof(bool)           ||
            typeof(T) == typeof(Guid)           ||
            typeof(T) == typeof(DateTime)       ||
            typeof(T) == typeof(DateTimeOffset) ||
            typeof(T) == typeof(TimeSpan)       ||
            false;

        public static bool TryFormat(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            return value switch
            {
                byte    x => x.TryFormat(destination, out charsWritten, format, provider),
                sbyte   x => x.TryFormat(destination, out charsWritten, format, provider),
                char    x => TryFormatChar(x, destination, out charsWritten),

                short   x => x.TryFormat(destination, out charsWritten, format, provider),
                ushort  x => x.TryFormat(destination, out charsWritten, format, provider),

                int     x => x.TryFormat(destination, out charsWritten, format, provider),
                uint    x => x.TryFormat(destination, out charsWritten, format, provider),

                long    x => x.TryFormat(destination, out charsWritten, format, provider),
                ulong   x => x.TryFormat(destination, out charsWritten, format, provider),

                float   x => x.TryFormat(destination, out charsWritten, format, provider),
                double  x => x.TryFormat(destination, out charsWritten, format, provider),

                bool    x => x.TryFormat(destination, out charsWritten),
                Guid    x => x.TryFormat(destination, out charsWritten, format),

                DateTime        x => x.TryFormat(destination, out charsWritten, format, provider),
                DateTimeOffset  x => x.TryFormat(destination, out charsWritten, format, provider),
                TimeSpan        x => x.TryFormat(destination, out charsWritten, format, provider),


                _ => Fallback(out charsWritten),
            };

            static bool Fallback(out int charsWritten)
            {
                charsWritten = 0;
                return false;
            }
        }

        private static bool TryFormatChar(char value, Span<char> destination, out int charsWritten)
        {
            if (!destination.IsEmpty)
            {
                destination[0] = value;
                charsWritten = 1;
                return true;
            }

            charsWritten = 0;
            return false;
        }
    }
}

internal sealed class InterpolatedStringHandlerAttribute : Attribute { }