using System.Text;

namespace EOLib.Data;

/// <summary>
/// A class for reading EO data from a sequence of bytes.
/// <para/>
/// <c>EoReader</c> features a chunked reading mode, which is important for accurate emulation of
/// the official game client.
/// <para/>
/// See: <see href="https://github.com/Cirras/eo-protocol/blob/master/docs/chunks.md">Chunked Reading</see>
/// </summary>
public sealed class EoReader
{
    private readonly byte[] _data;
    private readonly int _offset;
    private readonly int _limit;

    private bool _chunkedReadingMode;
    private int _chunkStart;
    private int _nextBreak;

    /// <summary>
    /// Gets or sets chunked reading mode for the reader.
    /// </summary>
    ///
    /// <remarks>
    /// In chunked reading mode, the reader will treat <c>0xFF</c> bytes as the end of the current chunk. <see cref="NextChunk" /> can be called to move to the next chunk.
    /// </remarks>
    public bool ChunkedReadingMode
    {
        get
        {
            return _chunkedReadingMode;
        }
        set
        {
            _chunkedReadingMode = value;
            if (_nextBreak == -1)
            {
                _nextBreak = FindNextBreakIndex();
            }
        }
    }

    /// <summary>
    /// Gets the number of bytes remaining.
    /// </summary>
    ///
    /// <remarks>
    /// If chunked reading mode is enabled, gets the number of bytes remaining in the current chunk. Otherwise, gets the total number of bytes remaining in the input data.
    /// </remarks>
    public int Remaining =>
        _chunkedReadingMode
            ? _nextBreak - Math.Min(Position, _nextBreak)
            : _limit - Position;

    /// <summary>
    /// Gets the current Position in the input data.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Creates a new <c>EoReader</c> instance for the specified data.
    /// </summary>
    /// <param name="data">The byte array containing the input data</paramref>
    public EoReader(byte[] data)
        : this(data, 0, data.Length) { }

    private EoReader(byte[] data, int offset, int limit)
    {
        _data = data;
        _offset = offset;
        _limit = limit;
        Position = 0;
        _chunkedReadingMode = false;
        _chunkStart = 0;
        _nextBreak = -1;
    }

    /// <summary>
    /// Creates a new <see cref="EoReader"/> whose input data is a shared subsequence of this reader's data.
    /// </summary>
    /// <remarks>
    /// The input data of the new reader will start at this reader's current position and contain
    /// all remaining data. The two reader's position and chunked reading mode will be independent.
    ///
    /// The new reader's position will be zero, and its chunked reading mode will be false.
    /// </remarks>
    /// <returns>The new reader.</returns>
    public EoReader Slice()
    {
        return Slice(Position);
    }

    /// <summary>
    /// Creates a new <see cref="EoReader"/> whose input data is a shared subsequence of this reader's data.
    /// </summary>
    /// <remarks>
    /// The input data of the new reader will start at position <paramref name="index"/> in this reader and
    /// contain all remaining data. The two reader's position and chunked reading mode will be independent.
    ///
    /// The new reader's position will be zero, and its chunked reading mode will be false.
    /// </remarks>
    /// <param name="index">The position in this reader at which the data of the new reader will start; must be non-negative.</param>
    /// <returns>The new reader.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="index"/> is negative. <br />
    /// This exception will <b>not</b> be thrown if <paramref name="index"/> is greater than the size of the
    /// input data. Consistent with the existing over-read behaviors, an empty reader will be
    /// returned.</exception>
    public EoReader Slice(int index)
    {
        return Slice(index, Math.Max(0, _limit - index));
    }

    /// <summary>
    /// Creates a new <see cref="EoReader"/> whose input data is a shared subsequence of this reader's data.
    /// </summary>
    ///
    /// <remarks>
    /// The input data of the new reader will start at position <paramref name="index"/> in this reader and
    /// contain up to <paramref name="length"/> bytes. The two reader's position and chunked reading mode will be
    /// independent.
    ///
    /// The new reader's position will be zero, and its chunked reading mode will be false.
    /// </remarks>
    ///
    /// <param name="index">The position in this reader at which the data of the new reader will start; must be non-negative.</param>
    /// <param name="length">The length of the shared subsequence of data to supply to the new reader; must be non-negative.</param>
    ///
    /// <returns>The new reader.</returns>
    ///
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="index"/> or <paramref name="length"/> is negative.
    /// <para>This exception will <b>not</b> be thrown if <paramref name="index"/> + <paramref name="length"/> is greater than the size
    /// of the input data. Consistent with the existing over-read behaviors, the new reader will be
    /// supplied a shared subsequence of all remaining data starting from <paramref name="index"/>.</para>
    /// </exception>
    public EoReader Slice(int index, int length)
    {
        if (index < 0)
        {
            throw new ArgumentException("Index must not be negative.", nameof(index));
        }

        if (length < 0)
        {
            throw new ArgumentException("Length must not be negative.", nameof(length));
        }

        int sliceOffset = Math.Max(0, Math.Min(_limit, index));
        int sliceLimit = Math.Min(_limit - sliceOffset, length);

        return new EoReader(_data, sliceOffset, sliceLimit);
    }

    /// <summary>
    /// Reads a raw byte from the input data.
    /// </summary>
    public int GetByte()
    {
        return ReadByte();
    }

    /// <summary>
    /// Reads an array of raw bytes from the input data.
    /// </summary>
    public byte[] GetBytes(int length)
    {
        return ReadBytes(length);
    }

    /// <summary>
    /// Reads an encoded 1-byte integer from the input data.
    /// </summary>
    public int GetChar()
    {
        return NumberEncoder.DecodeNumber(ReadBytes(1));
    }

    /// <summary>
    /// Reads an encoded 2-byte integer from the input data.
    /// </summary>
    public int GetShort()
    {
        return NumberEncoder.DecodeNumber(ReadBytes(2));
    }

    /// <summary>
    /// Reads an encoded 3-byte integer from the input data.
    /// </summary>
    public int GetThree()
    {
        return NumberEncoder.DecodeNumber(ReadBytes(3));
    }

    /// <summary>
    /// Reads an encoded 4-byte integer from the input data.
    /// </summary>
    public int GetInt()
    {
        return NumberEncoder.DecodeNumber(ReadBytes(4));
    }

    /// <summary>
    /// Reads a string from the input data.
    /// </summary>
    public string GetString()
    {
        byte[] bytes = ReadBytes(Remaining);
        return Encoding.GetEncoding(StringEncoder.Encoding).GetString(bytes);
    }

    /// <summary>
    /// Reads a string with a fixed length from the input data.
    /// </summary>
    /// <param name="length">The length of the string</param>
    /// <returns>A decoded string</returns>
    /// <exception cref="ArgumentException">If the length is negative</exception>
    public string GetFixedString(int length)
    {
        return GetFixedString(length, false);
    }

    /// <summary>
    /// Reads a string with a fixed length from the input data.
    /// </summary>
    /// <param name="length">The length of the string</param>
    /// <param name="padded">True if the string is padded with trailing <c>0xFF</c> bytes
    /// <returns>A decoded string</returns>
    /// <exception cref="ArgumentException">If the length is negative</exception>
    public string GetFixedString(int length, bool padded)
    {
        if (length < 0)
        {
            throw new ArgumentException("Length must not be negative.", nameof(length));
        }

        byte[] bytes = ReadBytes(length);
        if (padded)
        {
            bytes = RemovePadding(bytes);
        }

        return Encoding.GetEncoding(StringEncoder.Encoding).GetString(bytes);
    }

    /// <summary>
    /// Reads an encoded string from the input data.
    /// </summary>
    public string GetEncodedString()
    {
        byte[] bytes = ReadBytes(Remaining);
        StringEncoder.DecodeString(bytes);
        return Encoding.GetEncoding(StringEncoder.Encoding).GetString(bytes);
    }

    /// <summary>
    /// Reads an encoded string with a fixed length from the input data.
    /// </summary>
    /// <param name="length">The length of the string</param>
    /// <returns>A decoded string</returns>
    /// <exception cref="ArgumentException">If the length is negative</exception>
    public string GetFixedEncodedString(int length)
    {
        return GetFixedEncodedString(length, false);
    }

    /// <summary>
    /// Reads an encoded string with a fixed length from the input data.
    /// </summary>
    /// <param name="length">The length of the string</param>
    /// <param name="padded">True if the string is padded with trailing <c>0xFF</c> bytes
    /// <returns>A decoded string</returns>
    /// <exception cref="ArgumentException">If the length is negative</exception>
    public string GetFixedEncodedString(int length, bool padded)
    {
        if (length < 0)
        {
            throw new ArgumentException("Length must not be negative.", nameof(length));
        }

        byte[] bytes = ReadBytes(length);
        StringEncoder.DecodeString(bytes);
        if (padded)
        {
            bytes = RemovePadding(bytes);
        }
        return Encoding.GetEncoding(StringEncoder.Encoding).GetString(bytes);
    }

    /// <summary>
    /// Moves the reader position to the start of the next chunk in the input data.
    /// </summary>
    /// <exception cref="InvalidOperationException">If not in chunked reading mode</exception>
    public void NextChunk()
    {
        if (!ChunkedReadingMode)
        {
            throw new InvalidOperationException("Not in chunked reading mode.");
        }

        Position = _nextBreak;
        if (Position < _limit)
        {
            // Skip the break byte
            ++Position;
        }

        _chunkStart = Position;
        _nextBreak = FindNextBreakIndex();
    }

    private byte ReadByte()
    {
        if (Remaining > 0)
        {
            return _data[_offset + Position++];
        }
        return 0;
    }

    private byte[] ReadBytes(int length)
    {
        length = Math.Min(length, Remaining);

        byte[] result = new byte[length];
        Array.Copy(_data, _offset + Position, result, 0, length);

        Position += length;

        return result;
    }

    private static byte[] RemovePadding(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; ++i)
        {
            if (bytes[i] == 0xFF)
            {
                byte[] result = new byte[i];
                Array.Copy(bytes, 0, result, 0, i);
                return result;
            }
        }
        return bytes;
    }

    private int FindNextBreakIndex()
    {
        int i;
        for (i = _chunkStart; i < _limit; ++i)
        {
            if (_data[_offset + i] == 0xFF)
            {
                break;
            }
        }
        return i;
    }
}