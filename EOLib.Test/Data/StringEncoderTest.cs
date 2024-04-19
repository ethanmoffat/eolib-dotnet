using EOLib.Data;

namespace EOLib.Test.Data;

[TestFixture]
public class StringEncoderTest
{
    public static object[] TestArgs => new object[]
    {
          new object[] { "Hello, World!", "!;a-^H s^3a:)" },
          new object[] { "We're ¼ of the way there, so ¾ is remaining.", "C8_6_6l2h- ,d ¾ ^, sh-h7Y T>V h7Y g0 ¼ :[xhH"},
          new object[] { "64² = 4096", ";fAk b ²=i" },
          new object[] { "© FÒÖ BÃR BÅZ 2014", "=nAm EÅ] MÃ] ÖÒY ©" },
          new object[] { "Öxxö Xööx \"Lëïth Säë\" - \"Ÿ\"", "OŸO D OëäL 7YïëSO UööG öU'Ö" },
          new object[] { "Padded with 0xFFÿÿÿÿÿÿÿÿ", "ÿÿÿÿÿÿÿÿ+YUo 7Y6V i:i;lO" },
    };

    [TestCaseSource(nameof(TestArgs))]
    public void TestEncodeString(string input, string expected)
    {
        var bytes = ToBytes(input);
        StringEncoder.EncodeString(bytes);

        var encoded = FromBytes(bytes);
        Assert.That(encoded, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(TestArgs))]
    public void TestDecodeString(string expected, string input)
    {
        var bytes = ToBytes(input);
        StringEncoder.DecodeString(bytes);

        var decoded = FromBytes(bytes);
        Assert.That(decoded, Is.EqualTo(expected));
    }

    private static byte[] ToBytes(string str)
    {
        return StringEncoder.Encoding.GetBytes(str);
    }

    private static string FromBytes(byte[] bytes)
    {
        return StringEncoder.Encoding.GetString(bytes);
    }
}