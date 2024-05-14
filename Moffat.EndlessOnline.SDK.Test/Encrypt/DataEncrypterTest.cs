using Moffat.EndlessOnline.SDK.Data;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Moffat.EndlessOnline.SDK.Test.Encrypt;

[TestFixture]
public class DataEncrypterTest
{
    public static TestCaseData[] InterleaveSource => new TestCaseData[]
    {
        new TestCaseData("Hello, World!").Returns("H!edlllroo,W "),
        new TestCaseData("We're ¼ of the way there, so ¾ is remaining.").Returns("W.eg'nrien i¼a moefr  tshie  ¾w aoys  t,heer"),
        new TestCaseData("64² = 4096").Returns("6649²0 4= "),
        new TestCaseData("© FÒÖ BÃR BÅZ 2014").Returns("©4 1F0Ò2Ö  ZBÅÃBR "),
        new TestCaseData("Öxxö Xööx \"Lëïth Säë\" - \"Ÿ\"").Returns("Ö\"xŸx\"ö  -X ö\"öëxä S\" Lhëtï"),
        new TestCaseData("Padded with 0xFFÿÿÿÿÿÿÿÿ").Returns("Pÿaÿdÿdÿeÿdÿ ÿwÿiFtFhx 0"),
        new TestCaseData("This string contains NUL\0 (value 0) and a € (value 128)").Returns("T)h8i2s1  seturlianvg(  c€o nat adinnas  )N0U Le\0u l(av"),
    };

    public static TestCaseData[] DeinterleaveSource => new TestCaseData[]
    {
        new TestCaseData("Hello, World!").Returns("Hlo ol!drW,le"),
        new TestCaseData("We're ¼ of the way there, so ¾ is remaining.").Returns("W'e¼o h a hr,s  srmiig.nnae i¾o eetywetf  re"),
        new TestCaseData("64² = 4096").Returns("6²=4960  4"),
        new TestCaseData("© FÒÖ BÃR BÅZ 2014").Returns("©FÖBRBZ2140 Å Ã Ò "),
        new TestCaseData("Öxxö Xööx \"Lëïth Säë\" - \"Ÿ\"").Returns("Öx öx\"ët ä\"-\"\"Ÿ  ëShïL öXöx"),
        new TestCaseData("Padded with 0xFFÿÿÿÿÿÿÿÿ").Returns("Pde ih0FÿÿÿÿÿÿÿÿFx twdda"),
        new TestCaseData("This string contains NUL\0 (value 0) and a € (value 128)").Returns("Ti tigcnan U\0(au )ada€(au 2)81elv   n 0elv LNsito nrssh"),
    };

    public static TestCaseData[] FlipMsbSource => new TestCaseData[]
    {
        new TestCaseData("Hello, World!").Returns("Èåììï¬\u00A0×ïòìä¡"),
        new TestCaseData("We're ¼ of the way there, so ¾ is remaining.").Returns("×å§òå\u00A0<\u00A0ïæ\u00A0ôèå\u00A0÷áù\u00A0ôèåòå¬\u00A0óï\u00A0>\u00A0éó\u00A0òåíáéîéîç®"),
        new TestCaseData("64² = 4096").Returns("¶´2\u00A0½\u00A0´°¹¶"),
        new TestCaseData("© FÒÖ BÃR BÅZ 2014").Returns(")\u00A0ÆRV\u00A0ÂCÒ\u00A0ÂEÚ\u00A0²°±´"),
        new TestCaseData("Öxxö Xööx \"Lëïth Säë\" - \"Ÿ\"").Returns("Vøøv\u00A0Øvvø\u00A0¢Ìkoôè\u00A0Ódk¢\u00A0\u00AD\u00A0¢\u001F¢"),
        new TestCaseData("Padded with 0xFFÿÿÿÿÿÿÿÿ").Returns("Ðáääåä\u00A0÷éôè\u00A0°øÆÆ\u007F\u007F\u007F\u007F\u007F\u007F\u007F\u007F"),
        new TestCaseData("This string contains NUL\0 (value 0) and a € (value 128)").Returns("Ôèéó\u00A0óôòéîç\u00A0ãïîôáéîó\u00A0ÎÕÌ\0\u00A0¨öáìõå\u00A0°©\u00A0áîä\u00A0á\u00A0€\u00A0¨öáìõå\u00A0±²¸©"),
    };

    public static TestCaseData[] SwapMultiplesSource => new TestCaseData[]
    {
        new TestCaseData("Hello, World!", 3).Returns("Heoll, lroWd!"),
        new TestCaseData("Hello, World!", 0).Returns("Hello, World!"),
        new TestCaseData("We're ¼ of the way there, so ¾ is remaining.", 3).Returns("Wer'e ¼ fo the way there, so ¾ is remaining."),
        new TestCaseData("We're ¼ of the way there, so ¾ is remaining.", 0).Returns("We're ¼ of the way there, so ¾ is remaining."),
        new TestCaseData("64² = 4096", 3).Returns("64² = 4690"),
        new TestCaseData("64² = 4096", 0).Returns("64² = 4096"),
        new TestCaseData("© FÒÖ BÃR BÅZ 2014", 3).Returns("© FÒÖ ÃBR BÅZ 2014"),
        new TestCaseData("© FÒÖ BÃR BÅZ 2014", 0).Returns("© FÒÖ BÃR BÅZ 2014"),
        new TestCaseData("Öxxö Xööx \"Lëïth Säë\" - \"Ÿ\"", 3).Returns("Ööxx Xxöö \"Lëïth Säë\" - \"Ÿ\""),
        new TestCaseData("Öxxö Xööx \"Lëïth Säë\" - \"Ÿ\"", 0).Returns("Öxxö Xööx \"Lëïth Säë\" - \"Ÿ\""),
        new TestCaseData("Padded with 0xFFÿÿÿÿÿÿÿÿ", 3).Returns("Padded with x0FFÿÿÿÿÿÿÿÿ"),
        new TestCaseData("Padded with 0xFFÿÿÿÿÿÿÿÿ", 0).Returns("Padded with 0xFFÿÿÿÿÿÿÿÿ"),
        new TestCaseData("This string contains NUL\0 (value 0) and a € (value 128)", 3).Returns("This stirng ocntains NUL\0 (vaule 0) and a € (vaule 128)"),
        new TestCaseData("This string contains NUL\0 (value 0) and a € (value 128)", 0).Returns("This string contains NUL\0 (value 0) and a € (value 128)"),
    };

    [TestCaseSource(nameof(InterleaveSource))]
    public string TestInterleave_InterleavesBytes(string input)
    {
        return FromBytes(DataEncrypter.Interleave(ToBytes(input)));
    }

    [TestCaseSource(nameof(DeinterleaveSource))]
    public string TestDeinterleave_DeinterleavesBytes(string input)
    {
        return FromBytes(DataEncrypter.Deinterleave(ToBytes(input)));
    }

    [TestCaseSource(nameof(FlipMsbSource))]
    public string TestFlipMsb_FlipsMostSignificantBit(string input)
    {
        return FromBytes(DataEncrypter.FlipMSB(ToBytes(input)));
    }

    [TestCaseSource(nameof(SwapMultiplesSource))]
    public string TestSwapMultiples_SwapsBytesThatAreMultiplesOfValue(string input, int multiple)
    {
        return FromBytes(DataEncrypter.SwapMultiples(ToBytes(input), multiple));
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