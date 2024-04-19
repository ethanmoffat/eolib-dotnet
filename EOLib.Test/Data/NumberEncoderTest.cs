using EOLib.Data;

namespace EOLib.Test.Data;

[TestFixture]
public class NumberEncoderTest
{
    public static object[] TestArgs => new object[]
    {
          new object[] { 0, 0x01, 0xFE, 0xFE, 0xFE },
          new object[] { 1, 0x02, 0xFE, 0xFE, 0xFE },
          new object[] { 28, 0x1D, 0xFE, 0xFE, 0xFE },
          new object[] { 100, 0x65, 0xFE, 0xFE, 0xFE },
          new object[] { 128, 0x81, 0xFE, 0xFE, 0xFE },
          new object[] { 252, 0xFD, 0xFE, 0xFE, 0xFE },
          new object[] { 253, 0x01, 0x02, 0xFE, 0xFE },
          new object[] { 254, 0x02, 0x02, 0xFE, 0xFE },
          new object[] { 255, 0x03, 0x02, 0xFE, 0xFE },
          new object[] { 32003, 0x7E, 0x7F, 0xFE, 0xFE },
          new object[] { 32004, 0x7F, 0x7F, 0xFE, 0xFE },
          new object[] { 32005, 0x80, 0x7F, 0xFE, 0xFE },
          new object[] { 64008, 0xFD, 0xFD, 0xFE, 0xFE },
          new object[] { 64009, 0x01, 0x01, 0x02, 0xFE },
          new object[] { 64010, 0x02, 0x01, 0x02, 0xFE },
          new object[] { 10_000_000, 0xB0, 0x3A, 0x9D, 0xFE },
          new object[] { 16_194_276, 0xFD, 0xFD, 0xFD, 0xFE },
          new object[] { 16_194_277, 0x01, 0x01, 0x01, 0x02 },
          new object[] { 16_194_278, 0x02, 0x01, 0x01, 0x02 },
          new object[] { 2_048_576_039, 0x7E, 0x7F, 0x7F, 0x7F },
          new object[] { 2_048_576_040, 0x7F, 0x7F, 0x7F, 0x7F },
          new object[] { 2_048_576_041, 0x80, 0x7F, 0x7F, 0x7F },
          new object[] { unchecked((int) 4_097_152_079), 0xFC, 0xFD, 0xFD, 0xFD },
          new object[] { unchecked((int) 4_097_152_080), 0xFD, 0xFD, 0xFD, 0xFD },
    };

    [TestCaseSource(nameof(TestArgs))]
    public void TestEncodeNumber(int number, int b1, int b2, int b3, int b4)
    {
        Assert.That(
            NumberEncoder.EncodeNumber(number),
            Is.EqualTo(new byte[] { (byte)b1, (byte)b2, (byte)b3, (byte)b4 }));
    }

    [TestCaseSource(nameof(TestArgs))]
    public void TestDecodeNumber(int number, int b1, int b2, int b3, int b4)
    {
        Assert.That(
            NumberEncoder.DecodeNumber(new byte[] { (byte)b1, (byte)b2, (byte)b3, (byte)b4 }),
            Is.EqualTo(number));
    }
}