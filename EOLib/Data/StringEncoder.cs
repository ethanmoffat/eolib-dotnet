namespace EOLib.Data;

/// <summary>
/// Provides utility methods for encoding and decoding EO strings.
/// </summary>
public static class StringEncoder
{
    public const string Encoding = "windows-1252";

    /// <summary>
    /// Encodes a string by inverting the bytes and then reversing them.
    /// </summary>
    /// <remarks>
    /// This operation mutates the input array.
    /// </remarks>
    /// <param name="bytes">The byte array to encode.</param>
    public static void EncodeString(byte[] bytes)
    {
        InvertCharacters(bytes);
        ReverseCharacters(bytes);
    }

    /// <summary>
    /// Decodes a string by reversing the bytes and then inverting them.
    /// </summary>
    /// <remarks>
    /// This operation mutates the input array.
    /// </remarks>
    /// <param name="bytes">The byte array to decode.</param>
    public static void DecodeString(byte[] bytes)
    {
        ReverseCharacters(bytes);
        InvertCharacters(bytes);
    }

    private static void InvertCharacters(byte[] bytes)
    {
        bool flippy = (bytes.Length % 2 == 1);

        for (int i = 0; i < bytes.Length; ++i)
        {
            byte c = bytes[i];
            int f = 0;

            if (flippy)
            {
                f = 0x2E;
                if (c >= 0x50)
                {
                    f *= -1;
                }
            }

            if (c >= 0x22 && c <= 0x7E)
            {
                bytes[i] = (byte)(0x9F - c - f);
            }

            flippy = !flippy;
        }
    }

    private static void ReverseCharacters(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length / 2; i++)
        {
            byte b = bytes[i];
            bytes[i] = bytes[bytes.Length - i - 1];
            bytes[bytes.Length - i - 1] = b;
        }
    }
}
