namespace EOLib.Data;

using System;
using System.Text;

/// <summary>
/// A class for writing EO data to a sequence of bytes.
/// </summary>
public sealed class EoWriter
{
    private byte[] data = new byte[16];

    /// <summary>
    /// Gets a value indicating the length of the writer data
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Gets or sets a value toggling whether strings are sanitized for this writer.
    /// </summary>
    /// <remarks>
    /// With string sanitization enabled, the writer will switch <c>0xFF</c> bytes in strings (ÿ) to <c>0x79</c> (y).
    /// <para>
    /// See <see href="https://github.com/Cirras/eo-protocol/blob/master/docs/chunks.md#sanitization">Chunked Reading: Sanitization</see>
    /// </para>
    /// </remarks>
    public bool StringSanitization { get; set; }

    /// <summary>
    /// Adds a raw byte to the writer data.
    /// </summary>
    /// <param name="value">The byte to add.</param>
    /// <exception cref="ArgumentException">Thrown if the value is above 0xFF.</exception>
    public void AddByte(int value)
    {
        CheckNumberSize(value, 0xFF);

        if (Length + 1 > data.Length)
        {
            Expand(2);
        }

        data[Length++] = (byte)value;
    }

    /// <summary>
    /// Adds an array of raw bytes to the writer data.
    /// </summary>
    /// <param name="bytes">The array of bytes to add.</param>
    public void AddBytes(byte[] bytes)
    {
        AddBytes(bytes, bytes.Length);
    }

    /// <summary>
    /// Adds an encoded 1-byte integer to the writer data.
    /// </summary>
    /// <param name="number">The number to encode and add</param>
    /// <exception cref="ArgumentException">If the value is not below <see cref="EoNumericLimits.CHAR_MAX" /></exception>
    public void AddChar(int number)
    {
        CheckNumberSize(number, EoNumericLimits.CHAR_MAX - 1);
        byte[] bytes = NumberEncoder.EncodeNumber(number);
        AddBytes(bytes, 1);
    }

    /// <summary>
    /// Adds an encoded 2-byte integer to the writer data.
    /// </summary>
    /// <param name="number">The number to encode and add</param>
    /// <exception cref="ArgumentException">If the value is not below <see cref="EoNumericLimits.SHORT_MAX" /></exception>
    public void AddShort(int number)
    {
        CheckNumberSize(number, EoNumericLimits.SHORT_MAX - 1);
        byte[] bytes = NumberEncoder.EncodeNumber(number);
        AddBytes(bytes, 2);
    }

    /// <summary>
    /// Adds an encoded 3-byte integer to the writer data.
    /// </summary>
    /// <param name="number">The number to encode and add</param>
    /// <exception cref="ArgumentException">If the value is not below <see cref="EoNumericLimits.THREE_MAX" /></exception>
    public void AddThree(int number)
    {
        CheckNumberSize(number, EoNumericLimits.THREE_MAX - 1);
        byte[] bytes = NumberEncoder.EncodeNumber(number);
        AddBytes(bytes, 3);
    }

    /// <summary>
    /// Adds an encoded 4-byte integer to the writer data.
    /// </summary>
    /// <param name="number">The number to encode and add</param>
    /// <exception cref="ArgumentException">If the value is not below <see cref="EoNumericLimits.INT_MAX" /></exception>
    public void AddInt(int number)
    {
        CheckNumberSize(number, EoNumericLimits.INT_MAX - 1);
        byte[] bytes = NumberEncoder.EncodeNumber(number);
        AddBytes(bytes, 4);
    }

    /// <summary>
    /// Adds a string to the writer data.
    /// </summary>
    /// <param name="str">The string to be added</param>
    public void AddString(string str)
    {
        byte[] bytes = Encoding.GetEncoding(StringEncoder.Encoding).GetBytes(str);
        SanitizeString(bytes);
        AddBytes(bytes);
    }

    /// <summary>
    /// Adds a fixed-length string to the writer data.
    /// </summary>
    /// <param name="str">The string to be added</param>
    /// <param name="length">The expected length of the string</param>
    /// <exception cref="ArgumentException">If the string does not have the expected length</exception>
    public void AddFixedString(string str, int length)
    {
        AddFixedString(str, length, false);
    }

    /// <summary>
    /// Adds a fixed-length string to the writer data.
    /// </summary>
    /// <param name="str">The string to be added</param>
    /// <param name="length">The expected length of the string</param>
    /// <param name="padded">True if the string should be padded to the length with trailing <c>0xFF</c> bytes</param>
    /// <exception cref="ArgumentException">If the string does not have the expected length</exception>
    public void AddFixedString(string str, int length, bool padded)
    {
        CheckStringLength(str, length, padded);
        byte[] bytes = Encoding.GetEncoding(StringEncoder.Encoding).GetBytes(str);
        SanitizeString(bytes);
        if (padded)
        {
            bytes = AddPadding(bytes, length);
        }
        AddBytes(bytes);
    }

    /// <summary>
    /// Adds an encoded string to the writer data.
    /// </summary>
    /// <param name="str">The string to be encoded and added</param>
    public void AddEncodedString(string str)
    {
        byte[] bytes = Encoding.GetEncoding(StringEncoder.Encoding).GetBytes(str);
        SanitizeString(bytes);
        StringEncoder.EncodeString(bytes);
        AddBytes(bytes);
    }

    /// <summary>
    /// Adds a fixed-length encoded string to the writer data.
    /// </summary>
    /// <param name="str">The string to be encoded and added</param>
    /// <param name="length">The expected length of the string</param>
    /// <exception cref="ArgumentException">If the string does not have the expected length</exception>
    public void AddFixedEncodedString(string str, int length)
    {
        AddFixedEncodedString(str, length, false);
    }

    /// <summary>
    /// Adds a fixed-length encoded string to the writer data.
    /// </summary>
    /// <param name="str">The string to be encoded added</param>
    /// <param name="length">The expected length of the string</param>
    /// <param name="padded">True if the string should be padded to the length with trailing <c>0xFF</c> bytes</param>
    /// <exception cref="ArgumentException">If the string does not have the expected length</exception>
    public void AddFixedEncodedString(string str, int length, bool padded)
    {
        CheckStringLength(str, length, padded);
        byte[] bytes = Encoding.GetEncoding(StringEncoder.Encoding).GetBytes(str);
        SanitizeString(bytes);
        if (padded)
        {
            bytes = AddPadding(bytes, length);
        }
        StringEncoder.EncodeString(bytes);
        AddBytes(bytes);
    }

    /// <summary>
    /// Gets the writer data as a byte array.
    /// </summary>
    /// <returns>A copy of the writer data as a byte array</returns>
    public byte[] ToByteArray()
    {
        byte[] copy = new byte[Length];
        Array.Copy(data, 0, copy, 0, Length);
        return copy;
    }

    private void AddBytes(byte[] bytes, int bytesLength)
    {
        int expandFactor = 1;
        while (Length + bytesLength > data.Length * expandFactor)
        {
            expandFactor *= 2;
        }

        if (expandFactor > 1)
        {
            Expand(expandFactor);
        }

        Array.Copy(bytes, 0, data, Length, bytesLength);
        Length += bytesLength;
    }

    private void Expand(int expandFactor)
    {
        byte[] expanded = new byte[data.Length * expandFactor];
        Array.Copy(data, 0, expanded, 0, Length);
        data = expanded;
    }

    private void SanitizeString(byte[] bytes)
    {
        if (StringSanitization)
        {
            for (int i = 0; i < bytes.Length; ++i)
            {
                if (bytes[i] == (byte)0xFF) // ÿ
                {
                    bytes[i] = 0x79; // y
                }
            }
        }
    }

    private static byte[] AddPadding(byte[] bytes, int length)
    {
        if (bytes.Length == length)
        {
            return bytes;
        }

        byte[] result = new byte[length];
        Array.Copy(bytes, 0, result, 0, bytes.Length);
        for (int i = bytes.Length; i < length; ++i)
        {
            result[i] = 0xFF;
        }

        return result;
    }

    private static void CheckNumberSize(int number, int max)
    {
        if ((uint)number > (uint)max)
        {
            throw new ArgumentException($"Value {number} exceeds maximum of {max}.");
        }
    }

    private static void CheckStringLength(string str, int length, bool padded)
    {
        if (padded)
        {
            if (length >= str.Length)
            {
                return;
            }

            throw new ArgumentException($"Padded string \"{str}\" is too large for a length of {length}.");
        }

        if (str.Length != length)
        {
            throw new ArgumentException($"string \"{str}\" does not have expected length of {length}.");
        }
    }
}
