namespace EOLib.Data;

/// <summary>
/// A helper class for encrypting and decrypting EO data.
/// </summary>
public static class DataEncrypter
{
    /// <summary>
    /// Interleaves a sequence of bytes. When encrypting EO data, bytes are "woven" into each other.
    /// </summary>
    /// <remarks>
    /// Used when encrypting packets and data files.
    ///
    /// Example: <c>{0, 1, 2, 3, 4, 5} → {0, 5, 1, 4, 2, 3}</c>
    /// </remarks>
    /// <param name="data">The data to interleave</param>
    /// <returns>The interleaved data</returns>
    public static byte[] Interleave(byte[] data)
    {
        var numArray = new byte[data.Length];
        var index1 = 0;
        var num = 0;

        while (index1 < data.Length)
        {
            numArray[index1] = data[num++];
            index1 += 2;
        }

        var index2 = index1 - 1;
        if (data.Length % 2 != 0)
            index2 -= 2;

        while (index2 >= 0)
        {
            numArray[index2] = data[num++];
            index2 -= 2;
        }

        return numArray;
    }

    /// <summary>
    /// Deinterleaves a sequence of bytes. This is the reverse of <see cref="Interleave" />.
    /// </summary>
    /// <remarks>
    /// Used when decrypting packets and data files.
    ///
    /// Example: <c>{0, 1, 2, 3, 4, 5} → {0, 2, 4, 5, 3, 1}</c>
    /// </remarks>
    /// <param name="data">The data to deinterleave</param>
    /// <returns>The deinterleaved data</returns>
    public static byte[] Deinterleave(byte[] data)
    {
        var numArray = new byte[data.Length];
        var index1 = 0;
        var num = 0;

        while (index1 < data.Length)
        {
            numArray[num++] = data[index1];
            index1 += 2;
        }

        var index2 = index1 - 1;
        if (data.Length % 2 != 0)
            index2 -= 2;

        while (index2 >= 0)
        {
            numArray[num++] = data[index2];
            index2 -= 2;
        }

        return numArray;
    }

    /// <summary>
    /// Flips the most significant bit of each byte in a sequence of bytes. Values <c>0</c> and <c>128</c> are not flipped.
    /// </summary>
    /// <remarks>
    /// Used when encrypting and decrypting packets.
    ///
    /// Example: <c>{0, 1, 127, 128, 129, 254, 255} → {0, 129, 255, 128, 1, 126, 127}</c>
    /// </remarks>
    /// <param name="data">The data to flip most significant bits on</param>
    /// <returns>The modified data</returns>
    public static byte[] FlipMSB(byte[] data)
    {
        var retData = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            retData[i] = data[i] == 0x80 || data[i] == 0x00
                ? data[i]
                : (byte)(data[i] ^ 0x80u);
        }
        return retData;
    }

    /// <summary>
    /// Swaps the order of contiguous bytes in a sequence of bytes that are divisible by a given multiple value.
    /// </summary>
    /// <remarks>
    /// Used when encrypting and decrypting packets and data files.
    ///
    /// Example: for multi=3, <c>{10, 21, 27} → {10, 27, 21}</c>
    /// </remarks>
    /// <param name="data">The data to swap bytes in</param>
    /// <param name="multi">The multiple value</param>
    /// <returns>The modified data</returns>
    public static byte[] SwapMultiples(byte[] data, int multi)
    {
        int num1 = 0;

        var result = new byte[data.Length];
        Array.Copy(data, result, data.Length);

        for (int index1 = 0; index1 <= data.Length; ++index1)
        {
            if (index1 != data.Length && data[index1] % multi == 0)
            {
                ++num1;
            }
            else
            {
                if (num1 > 1)
                {
                    for (int index2 = 0; index2 < num1 / 2; ++index2)
                    {
                        byte num2 = data[index1 - num1 + index2];
                        result[index1 - num1 + index2] = data[index1 - index2 - 1];
                        result[index1 - index2 - 1] = num2;
                    }
                }
                num1 = 0;
            }
        }

        return result;
    }
}
