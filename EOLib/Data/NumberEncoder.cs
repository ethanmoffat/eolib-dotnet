namespace EOLib.Data;

/// <summary>
/// Provides utility methods for encoding and decoding EO numbers.
/// </summary>
public static class NumberEncoder
{
    /// <summary>
    /// Encodes a number to a sequence of bytes.
    /// </summary>
    /// <param name="number">The number to encode.</param>
    /// <returns>The encoded sequence of bytes.</returns>
    public static byte[] EncodeNumber(int number)
    {
        var value = (uint)number;
        var original = value;

        uint d = 0xFE;
        if (original >= EoNumericLimits.THREE_MAX)
        {
            d = value / EoNumericLimits.THREE_MAX + 1;
            value %= EoNumericLimits.THREE_MAX;
        }

        uint c = 0xFE;
        if (original >= EoNumericLimits.SHORT_MAX)
        {
            c = value / EoNumericLimits.SHORT_MAX + 1;
            value %= EoNumericLimits.SHORT_MAX;
        }

        uint b = 0xFE;
        if (original >= EoNumericLimits.CHAR_MAX)
        {
            b = value / EoNumericLimits.CHAR_MAX + 1;
            value %= EoNumericLimits.CHAR_MAX;
        }

        uint a = value + 1;

        return new byte[] { (byte)a, (byte)b, (byte)c, (byte)d };
    }

    /// <summary>
    /// Decodes a number from a sequence of bytes.
    /// </summary>
    /// <param name="bytes">The sequence of bytes to decode.</param>
    /// <returns>The decoded number.</returns>
    public static int DecodeNumber(byte[] bytes)
    {
        int result = 0;
        int length = Math.Min(bytes.Length, 4);

        for (int i = 0; i < length; ++i)
        {
            byte b = bytes[i];

            if (b == 0xFE)
            {
                break;
            }

            int value = b - 1;

            switch (i)
            {
                case 0:
                    result += value;
                    break;
                case 1:
                    result += (int)EoNumericLimits.CHAR_MAX * value;
                    break;
                case 2:
                    result += (int)EoNumericLimits.SHORT_MAX * value;
                    break;
                case 3:
                    result += (int)EoNumericLimits.THREE_MAX * value;
                    break;
            }
        }

        return result;
    }
}
