using Moffat.EndlessOnline.SDK.Data;

namespace Moffat.EndlessOnline.SDK.Test.Data;

[TestFixture]
public class EoReaderTest
{
    [Test]
    public void TestSlice_CreatesReaderWithIndependentLength()
    {
        var reader = CreateReader(0x01, 0x02, 0x03, 0x04, 0x05, 0x06);
        reader.GetByte();
        reader.ChunkedReadingMode = true;

        var reader2 = reader.Slice();

        Assert.Multiple(() =>
        {
            Assert.That(reader2.Position, Is.Zero);
            Assert.That(reader2.Remaining, Is.EqualTo(5));
            Assert.That(reader2.ChunkedReadingMode, Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Position, Is.EqualTo(1));
            Assert.That(reader.Remaining, Is.EqualTo(5));
            Assert.That(reader.ChunkedReadingMode, Is.True);
        });
    }

    [Test]
    public void TestSlice_WithInputIndex_CreatesReaderWithIndependentLength()
    {
        var reader = CreateReader(0x01, 0x02, 0x03, 0x04, 0x05, 0x06);
        reader.GetByte();
        reader.ChunkedReadingMode = true;

        var reader2 = reader.Slice();
        var reader3 = reader2.Slice(1);

        Assert.Multiple(() =>
        {
            Assert.That(reader3.Position, Is.Zero);
            Assert.That(reader3.Remaining, Is.EqualTo(4));
            Assert.That(reader3.ChunkedReadingMode, Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Position, Is.EqualTo(1));
            Assert.That(reader.Remaining, Is.EqualTo(5));
            Assert.That(reader.ChunkedReadingMode, Is.True);
        });
    }

    [Test]
    public void TestSlice_WithInputIndexAndSize_CreatesReaderWithIndependentLength()
    {
        var reader = CreateReader(0x01, 0x02, 0x03, 0x04, 0x05, 0x06);
        reader.GetByte();
        reader.ChunkedReadingMode = true;

        var reader2 = reader.Slice();
        var reader3 = reader2.Slice(1);
        var reader4 = reader3.Slice(1, 2);

        Assert.Multiple(() =>
        {
            Assert.That(reader4.Position, Is.Zero);
            Assert.That(reader4.Remaining, Is.EqualTo(2));
            Assert.That(reader4.ChunkedReadingMode, Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(reader.Position, Is.EqualTo(1));
            Assert.That(reader.Remaining, Is.EqualTo(5));
            Assert.That(reader.ChunkedReadingMode, Is.True);
        });
    }

    [Test]
    public void TestSlice_OverRead_DoesNotSeekPastEnd()
    {
        var reader = CreateReader(0x01, 0x02, 0x03);
        Assert.Multiple(() =>
        {
            Assert.That(reader.Slice(2, 5).Remaining, Is.EqualTo(1));
            Assert.That(reader.Slice(3).Remaining, Is.Zero);
            Assert.That(reader.Slice(4).Remaining, Is.Zero);
            Assert.That(reader.Slice(4, 12345).Remaining, Is.Zero);
        });
    }

    [Test]
    public void TestSlice_NegativeIndex_Throws()
    {
        var reader = CreateReader(0x01, 0x02, 0x03);
        Assert.Throws<ArgumentException>(() => reader.Slice(-1));
    }

    [Test]
    public void TestSlice_NegativeLength_Throws()
    {
        EoReader reader = CreateReader(0x01, 0x02, 0x03);
        Assert.Throws<ArgumentException>(() => reader.Slice(0, -1));
    }

    [TestCase(0x00, ExpectedResult = 0x00)]
    [TestCase(0x01, ExpectedResult = 0x01)]
    [TestCase(0x02, ExpectedResult = 0x02)]
    [TestCase(0x80, ExpectedResult = 0x80)]
    [TestCase(0xFD, ExpectedResult = 0xFD)]
    [TestCase(0xFE, ExpectedResult = 0xFE)]
    [TestCase(0xFF, ExpectedResult = 0xFF)]
    public int TestGetByte_GetsNextByte(int inputByte)
    {
        var reader = CreateReader(inputByte);
        return reader.GetByte();
    }

    [Test]
    public void TestGetByte_OverRead_DoesNotSeekPastEnd()
    {
        var reader = CreateReader();
        Assert.That(reader.GetByte(), Is.EqualTo(0));
    }

    [Test]
    public void TestGetBytes_ReadsMultipleBytesFromReader()
    {
        var reader = CreateReader(0x01, 0x02, 0x03, 0x04, 0x05);
        Assert.Multiple(() =>
        {
            Assert.That(reader.GetBytes(3), Is.EqualTo(new byte[] { 0x01, 0x02, 0x03 }));
            Assert.That(reader.GetBytes(10), Is.EqualTo(new byte[] { 0x04, 0x05 }));
            Assert.That(reader.GetBytes(1), Is.Empty);
        });
    }

    [Test]
    public void TestGetChar_GetsAndDecodesNextChar()
    {
        var reader = CreateReader(0x01, 0x02, 0x80, 0x81, 0xFD, 0xFE, 0xFF);
        Assert.That(reader.GetChar(), Is.Zero);
        Assert.That(reader.GetChar(), Is.EqualTo(1));
        Assert.That(reader.GetChar(), Is.EqualTo(127));
        Assert.That(reader.GetChar(), Is.EqualTo(128));
        Assert.That(reader.GetChar(), Is.EqualTo(252));
        Assert.That(reader.GetChar(), Is.Zero);
        Assert.That(reader.GetChar(), Is.EqualTo(254));
    }

    [Test]
    public void TestGetShort_GetsAndDecodesNextShort()
    {
        var reader = CreateReader(
            0x01, 0xFE, 0x02, 0xFE,
            0x80, 0xFE, 0xFD, 0xFE,
            0xFE, 0xFE, 0xFE, 0x80,
            0x7F, 0x7F, 0xFD, 0xFD);

        Assert.That(reader.GetShort(), Is.Zero);
        Assert.That(reader.GetShort(), Is.EqualTo(1));
        Assert.That(reader.GetShort(), Is.EqualTo(127));
        Assert.That(reader.GetShort(), Is.EqualTo(252));
        Assert.That(reader.GetShort(), Is.Zero);
        Assert.That(reader.GetShort(), Is.Zero);
        Assert.That(reader.GetShort(), Is.EqualTo(32004));
        Assert.That(reader.GetShort(), Is.EqualTo(64008));
    }

    [Test]
    public void TestGetThree_GetsAndDecodesNextThree()
    {
        var reader = CreateReader(
                0x01, 0xFE, 0xFE, 0x02, 0xFE, 0xFE,
                0x80, 0xFE, 0xFE, 0xFD, 0xFE, 0xFE,
                0xFE, 0xFE, 0xFE, 0xFE, 0x80, 0x81,
                0x7F, 0x7F, 0xFE, 0xFD, 0xFD, 0xFE, 0xFD, 0xFD, 0xFD);
        Assert.That(reader.GetThree(), Is.Zero);
        Assert.That(reader.GetThree(), Is.EqualTo(1));
        Assert.That(reader.GetThree(), Is.EqualTo(127));
        Assert.That(reader.GetThree(), Is.EqualTo(252));
        Assert.That(reader.GetThree(), Is.Zero);
        Assert.That(reader.GetThree(), Is.Zero);
        Assert.That(reader.GetThree(), Is.EqualTo(32004));
        Assert.That(reader.GetThree(), Is.EqualTo(64008));
        Assert.That(reader.GetThree(), Is.EqualTo(16194276));
    }

    [Test]
    public void TestGetInt_GetsAndDecodesNextInt()
    {
        var reader = CreateReader(
                0x01, 0xFE, 0xFE, 0xFE, 0x02, 0xFE, 0xFE, 0xFE,
                0x80, 0xFE, 0xFE, 0xFE, 0xFD, 0xFE, 0xFE, 0xFE,
                0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0x80, 0x81, 0x82,
                0x7F, 0x7F, 0xFE, 0xFE, 0xFD, 0xFD, 0xFE, 0xFE,
                0xFD, 0xFD, 0xFD, 0xFE, 0x7F, 0x7F, 0x7F, 0x7F,
                0xFD, 0xFD, 0xFD, 0xFD);
        Assert.That(reader.GetInt(), Is.Zero);
        Assert.That(reader.GetInt(), Is.EqualTo(1));
        Assert.That(reader.GetInt(), Is.EqualTo(127));
        Assert.That(reader.GetInt(), Is.EqualTo(252));
        Assert.That(reader.GetInt(), Is.Zero);
        Assert.That(reader.GetInt(), Is.Zero);
        Assert.That(reader.GetInt(), Is.EqualTo(32004));
        Assert.That(reader.GetInt(), Is.EqualTo(64008));
        Assert.That(reader.GetInt(), Is.EqualTo(16194276));
        Assert.That(reader.GetInt(), Is.EqualTo(2_048_576_040));
        Assert.That(reader.GetInt(), Is.EqualTo(unchecked((int)4_097_152_080)));
    }

    [Test]
    public void TestGetString()
    {
        var reader = CreateReader("Hello, World!");
        Assert.That(reader.GetString(), Is.EqualTo("Hello, World!"));
    }

    [Test]
    public void TestGetFixedString()
    {
        var reader = CreateReader("foobar");
        Assert.That(reader.GetFixedString(3), Is.EqualTo("foo"));
        Assert.That(reader.GetFixedString(3), Is.EqualTo("bar"));
    }

    [Test]
    public void TestGetPaddedFixedString()
    {
        var reader = CreateReader("fooÿbarÿÿÿ");
        Assert.That(reader.GetFixedString(4, true), Is.EqualTo("foo"));
        Assert.That(reader.GetFixedString(6, true), Is.EqualTo("bar"));
    }

    [Test]
    public void TestChunkedGetString_GetsStringForCurrentChunk()
    {
        var reader = CreateReader("Hello,ÿWorld!");
        reader.ChunkedReadingMode = true;

        Assert.That(reader.GetString(), Is.EqualTo("Hello,"));

        reader.NextChunk();
        Assert.That(reader.GetString(), Is.EqualTo("World!"));
    }

    [Test]
    public void TestGetNegativeLengthString_Throws()
    {
        var reader = CreateReader("foo");
        Assert.Throws<ArgumentException>(() => reader.GetFixedString(-1));
    }

    [Test]
    public void TestGetEncodedString()
    {
        var reader = CreateReader("!;a-^H s^3a:)");
        Assert.That(reader.GetEncodedString(), Is.EqualTo("Hello, World!"));
    }

    [Test]
    public void TestGetFixedEncodedString()
    {
        var reader = CreateReader("^0g[>k");
        Assert.That(reader.GetFixedEncodedString(3), Is.EqualTo("foo"));
        Assert.That(reader.GetFixedEncodedString(3), Is.EqualTo("bar"));
    }

    [Test]
    public void TestGetPaddedFixedEncodedString()
    {
        var reader = CreateReader("ÿ0^9ÿÿÿ-l=S>k");
        Assert.That(reader.GetFixedEncodedString(4, true), Is.EqualTo("foo"));
        Assert.That(reader.GetFixedEncodedString(6, true), Is.EqualTo("bar"));
        Assert.That(reader.GetFixedEncodedString(3, true), Is.EqualTo("baz"));
    }

    [Test]
    public void TestChunkedGetEncodedString_GetsStringForCurrentChunk()
    {
        var reader = CreateReader("E0a3hWÿ!;a-^H");
        reader.ChunkedReadingMode = true;

        Assert.That(reader.GetEncodedString(), Is.EqualTo("Hello,"));

        reader.NextChunk();
        Assert.That(reader.GetEncodedString(), Is.EqualTo("World!"));
    }

    [Test]
    public void TestGetNegativeLengthEncodedString_Throws()
    {
        var reader = CreateReader("^0g");
        Assert.Throws<ArgumentException>(() => reader.GetFixedEncodedString(-1));
    }

    [Test]
    public void TestRemaining_HasCorrectValue()
    {
        var reader = CreateReader(0x01, 0x03, 0x04, 0xFE, 0x05, 0xFE, 0xFE, 0x06, 0xFE, 0xFE, 0xFE);
        Assert.That(reader.Remaining, Is.EqualTo(11));

        reader.GetByte();
        Assert.That(reader.Remaining, Is.EqualTo(10));

        reader.GetChar();
        Assert.That(reader.Remaining, Is.EqualTo(9));

        reader.GetShort();
        Assert.That(reader.Remaining, Is.EqualTo(7));

        reader.GetThree();
        Assert.That(reader.Remaining, Is.EqualTo(4));

        reader.GetInt();
        Assert.That(reader.Remaining, Is.Zero);

        reader.GetChar();
        Assert.That(reader.Remaining, Is.Zero);
    }

    [Test]
    public void TestChunkedRemaining_HasValueForCurrentChunkSize()
    {
        var reader = CreateReader(
            0x01, 0x03, 0x04,
            0xFF,
            0x05, 0xFE, 0xFE, 0x06, 0xFE, 0xFE, 0xFE);
        Assert.That(reader.Remaining, Is.EqualTo(11));

        reader.ChunkedReadingMode = true;
        Assert.That(reader.Remaining, Is.EqualTo(3));

        reader.GetChar();
        reader.GetShort();
        Assert.That(reader.Remaining, Is.Zero);

        reader.GetChar();
        Assert.That(reader.Remaining, Is.Zero);

        reader.NextChunk();
        Assert.That(reader.Remaining, Is.EqualTo(7));
    }

    [Test]
    public void TestNextChunk_MovesToNextChunk_DelimitedByMaxValueByte()
    {
        var reader = CreateReader(0x01, 0x02, 0xFF, 0x03, 0x04, 0x5, 0xFF, 0x06);

        reader.ChunkedReadingMode = true;
        Assert.That(reader.Position, Is.Zero);

        reader.NextChunk();
        Assert.That(reader.Position, Is.EqualTo(3));

        reader.NextChunk();
        Assert.That(reader.Position, Is.EqualTo(7));

        reader.NextChunk();
        Assert.That(reader.Position, Is.EqualTo(8));

        reader.NextChunk();
        Assert.That(reader.Position, Is.EqualTo(8));
    }

    [Test]
    public void TestNextChunk_NotInChunkedReadingMode_Throws()
    {
        var reader = CreateReader(0x01, 0x02, 0xFF, 0x03, 0x04, 0x5, 0xFF, 0x06);
        Assert.Throws<InvalidOperationException>(() => reader.NextChunk());
    }

    [Test]
    public void TestChunkedReading_ToggledBetweenReads_SetsExpectedPosition()
    {
        var reader = CreateReader(0x01, 0x02, 0xFF, 0x03, 0x04, 0x5, 0xFF, 0x06);
        Assert.That(reader.Position, Is.Zero);

        reader.ChunkedReadingMode = true;
        reader.NextChunk();
        reader.ChunkedReadingMode = false;
        Assert.That(reader.Position, Is.EqualTo(3));

        reader.ChunkedReadingMode = true;
        reader.NextChunk();
        reader.ChunkedReadingMode = false;
        Assert.That(reader.Position, Is.EqualTo(7));

        reader.ChunkedReadingMode = true;
        reader.NextChunk();
        reader.ChunkedReadingMode = false;
        Assert.That(reader.Position, Is.EqualTo(8));

        reader.ChunkedReadingMode = true;
        reader.NextChunk();
        reader.ChunkedReadingMode = false;
        Assert.That(reader.Position, Is.EqualTo(8));
    }

    [Test]
    public void TestChunkedReading_ReadLessThanFullChunk_IgnoresGarbageData()
    {
        // See: https://github.com/Cirras/eo-protocol/blob/master/docs/chunks.md#1-under-read
        EoReader reader =
            CreateReader(0x7C, 0x67, 0x61, 0x72, 0x62, 0x61, 0x67, 0x65, 0xFF, 0xCA, 0x31);
        reader.ChunkedReadingMode = true;

        Assert.That(reader.GetChar(), Is.EqualTo(123));
        reader.NextChunk();
        Assert.That(reader.GetShort(), Is.EqualTo(12345));
    }

    [Test]
    public void TestChunkedReading_ReadMoreThanChunk_TruncatedResult()
    {
        // See: https://github.com/Cirras/eo-protocol/blob/master/docs/chunks.md#2-over-read
        EoReader reader = CreateReader(0xFF, 0x7C);
        reader.ChunkedReadingMode = true;

        Assert.That(reader.GetInt(), Is.Zero);
        reader.NextChunk();
        Assert.That(reader.GetShort(), Is.EqualTo(123));
    }

    [Test]
    public void TestChunkedReading_DoubleRead_ReadsExpectedData()
    {
        // See: https://github.com/Cirras/eo-protocol/blob/master/docs/chunks.md#3-double-read
        EoReader reader = CreateReader(0xFF, 0x7C, 0xCA, 0x31);

        // Reading all 4 bytes of the input data
        Assert.That(reader.GetInt(), Is.EqualTo(790222478));

        // Activating chunked mode and seeking to the first break byte with NextChunk(), which actually
        // takes our reader position backwards.
        reader.ChunkedReadingMode = true;
        reader.NextChunk();
        Assert.Multiple(() =>
        {
            Assert.That(reader.GetChar(), Is.EqualTo(123));
            Assert.That(reader.GetShort(), Is.EqualTo(12345));
        });
    }

    private static EoReader CreateReader(string str)
    {
        var data = StringEncoder.Encoding.GetBytes(str);
        return new EoReader(data);
    }

    private static EoReader CreateReader(params int[] bytes)
    {
        var data = new byte[bytes.Length + 20];
        for (int i = 0; i < bytes.Length; ++i)
        {
            data[10 + i] = (byte)bytes[i];
        }
        return new EoReader(data).Slice(10, bytes.Length);
    }
}
