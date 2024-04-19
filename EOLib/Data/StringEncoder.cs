using System.Text;

namespace EOLib.Data;

/// <summary>
/// Provides utility methods for encoding and decoding EO strings.
/// </summary>
public static class StringEncoder
{
    static StringEncoder()
    {
        Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252) ?? throw new InvalidOperationException("Unable to load Windows-1252 code page.");
    }

    public static Encoding Encoding { get; }

    /// <summary>
    /// Encodes a string by inverting the bytes and then reversing them.
    /// </summary>
    /// <param name="bytes">The byte array to encode.</param>
    /// <returns>The encoded string</returns>
    public static byte[] EncodeString(byte[] bytes)
    {
        return ReverseCharacters(InvertCharacters(bytes));
    }

    /// <summary>
    /// Decodes a string by reversing the bytes and then inverting them.
    /// </summary>
    /// <param name="bytes">The byte array to decode.</param>
    /// <returns>The decoded string</returns>
    public static byte[] DecodeString(byte[] bytes)
    {
        return InvertCharacters(ReverseCharacters(bytes));
    }

    private static byte[] InvertCharacters(byte[] bytes)
    {
        var output = new byte[bytes.Length];
        Array.Copy(bytes, output, bytes.Length);

        bool flippy = output.Length % 2 == 1;

        for (int i = 0; i < output.Length; ++i)
        {
            byte c = output[i];
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
                output[i] = (byte)(0x9F - c - f);
            }

            flippy = !flippy;
        }

        return output;
    }

    private static byte[] ReverseCharacters(byte[] bytes)
    {
        var output = new byte[bytes.Length];
        Array.Copy(bytes, output, bytes.Length);

        for (int i = 0; i < output.Length / 2; i++)
        {
            byte b = output[i];
            output[i] = output[output.Length - i - 1];
            output[output.Length - i - 1] = b;
        }

        return output;
    }
}
