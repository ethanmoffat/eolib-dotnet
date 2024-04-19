using EOLib.Data;

namespace EOLib.Test.Data;

[TestFixture]
public class EoWriterTest
{
    [Test]
    public void TestAddByte()
    {
        var writer = new EoWriter();
        writer.AddByte(0x00);
        Assert.That(writer.ToByteArray(), Is.EqualTo(new byte[] { 0x00 }));
    }

    [Test]
    public void TestAddBytes()
    {
        var writer = new EoWriter();
        writer.AddBytes(new byte[] { (byte)0x00, (byte)0xFF });
        Assert.That(writer.ToByteArray(), Is.EqualTo(new byte[] { 0x00, 0xFF }));
    }

    [Test]
    public void TestAddChar()
    {
        var writer = new EoWriter();
        writer.AddChar(123);
        Assert.That(writer.ToByteArray(), Is.EqualTo(new byte[] { 0x7C }));
    }

    [Test]
    public void TestAddShort()
    {
        var writer = new EoWriter();
        writer.AddShort(12345);
        Assert.That(writer.ToByteArray(), Is.EqualTo(new byte[] { 0xCA, 0x31 }));
    }

    [Test]
    public void TestAddThree()
    {
        var writer = new EoWriter();
        writer.AddThree(10_000_000);
        Assert.That(writer.ToByteArray(), Is.EqualTo(new byte[] { 0xB0, 0x3A, 0x9D }));
    }

    [Test]
    public void TestAddInt()
    {
        var writer = new EoWriter();
        writer.AddInt(2_048_576_040);
        Assert.That(writer.ToByteArray(), Is.EqualTo(new byte[] { 0x7F, 0x7F, 0x7F, 0x7F }));
    }

    [Test]
    public void TestAddString()
    {
        var writer = new EoWriter();
        writer.AddString("foo");
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("foo")));
    }

    [Test]
    public void TestAddFixedString()
    {
        var writer = new EoWriter();
        writer.AddFixedString("bar", 3);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("bar")));
    }

    [Test]
    public void TestAddPAddedFixedString()
    {
        var writer = new EoWriter();
        writer.AddFixedString("bar", 6, true);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("barÿÿÿ")));
    }

    [Test]
    public void TestAddPAddedWithPerfectFitFixedString()
    {
        var writer = new EoWriter();
        writer.AddFixedString("bar", 3, true);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("bar")));
    }

    [Test]
    public void TestAddEncodedString()
    {
        var writer = new EoWriter();
        writer.AddEncodedString("foo");
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("^0g")));
    }

    [Test]
    public void TestAddFixedEncodedString()
    {
        var writer = new EoWriter();
        writer.AddFixedEncodedString("bar", 3);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("[>k")));
    }

    [Test]
    public void TestAddPAddedFixedEncodedString()
    {
        var writer = new EoWriter();
        writer.AddFixedEncodedString("bar", 6, true);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("ÿÿÿ-l=")));
    }

    [Test]
    public void TestAddPAddedWithPerfectFitFixedEncodedString()
    {
        var writer = new EoWriter();
        writer.AddFixedEncodedString("bar", 3, true);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("[>k")));
    }

    [Test]
    public void TestAddSanitizedString()
    {
        var writer = new EoWriter { StringSanitization = true };
        writer.AddString("aÿz");
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("ayz")));
    }

    [Test]
    public void TestAddSanitizedFixedString()
    {
        var writer = new EoWriter { StringSanitization = true };
        writer.AddFixedString("aÿz", 3);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("ayz")));
    }

    [Test]
    public void TestAddSanitizedPAddedFixedString()
    {
        var writer = new EoWriter { StringSanitization = true };
        writer.AddFixedString("aÿz", 6, true);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("ayzÿÿÿ")));
    }

    [Test]
    public void TestAddSanitizedEncodedString()
    {
        var writer = new EoWriter { StringSanitization = true };
        writer.AddEncodedString("aÿz");
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("S&l")));
    }

    [Test]
    public void TestAddSanitizedFixedEncodedString()
    {
        var writer = new EoWriter { StringSanitization = true };
        writer.AddFixedEncodedString("aÿz", 3);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("S&l")));
    }

    [Test]
    public void TestAddSanitizedPAddedFixedEncodedString()
    {
        var writer = new EoWriter { StringSanitization = true };
        writer.AddFixedEncodedString("aÿz", 6, true);
        Assert.That(writer.ToByteArray(), Is.EqualTo(ToBytes("ÿÿÿ%T>")));
    }

    [Test]
    public void TestAddNumbersOnBoundary()
    {
        var writer = new EoWriter();
        Assert.Multiple(() =>
        {
            Assert.That(() => writer.AddByte(0xFF), Throws.Nothing);
            Assert.That(() => writer.AddChar((int)EoNumericLimits.CHAR_MAX - 1), Throws.Nothing);
            Assert.That(() => writer.AddShort((int)EoNumericLimits.SHORT_MAX - 1), Throws.Nothing);
            Assert.That(() => writer.AddThree((int)EoNumericLimits.THREE_MAX - 1), Throws.Nothing);
            Assert.That(() => writer.AddInt(unchecked((int)EoNumericLimits.INT_MAX - 1)), Throws.Nothing);
        });
    }

    [Test]
    public void TestAddNumbersExceedingLimit()
    {
        var writer = new EoWriter();
        Assert.Throws<ArgumentException>(() => writer.AddByte(256));
        Assert.Throws<ArgumentException>(() => writer.AddChar((int)EoNumericLimits.CHAR_MAX));
        Assert.Throws<ArgumentException>(() => writer.AddShort((int)EoNumericLimits.SHORT_MAX));
        Assert.Throws<ArgumentException>(() => writer.AddThree((int)EoNumericLimits.THREE_MAX));
        Assert.Throws<ArgumentException>(() => writer.AddInt(unchecked((int)EoNumericLimits.INT_MAX)));
    }

    [Test]
    public void TestAddFixedStringWithIncorrectLength()
    {
        var writer = new EoWriter();
        Assert.Throws<ArgumentException>(() => writer.AddFixedString("foo", 2));
        Assert.Throws<ArgumentException>(() => writer.AddFixedString("foo", 2, true));
        Assert.Throws<ArgumentException>(() => writer.AddFixedString("foo", 4));
        Assert.Throws<ArgumentException>(() => writer.AddFixedEncodedString("foo", 2));
        Assert.Throws<ArgumentException>(() => writer.AddFixedEncodedString("foo", 2, true));
        Assert.Throws<ArgumentException>(() => writer.AddFixedEncodedString("foo", 4));
    }

    [Test]
    public void TestGetLength()
    {
        var writer = new EoWriter();
        Assert.That(writer.Length, Is.Zero);

        writer.AddString("Lorem ipsum dolor sit amet");
        Assert.That(writer.Length, Is.EqualTo(26));

        for (int i = 27; i <= 100; ++i)
        {
            writer.AddByte(0xFF);
        }
        Assert.That(writer.Length, Is.EqualTo(100));
    }

    private static byte[] ToBytes(string str) => StringEncoder.Encoding.GetBytes(str);
}
