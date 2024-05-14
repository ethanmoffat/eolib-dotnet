using Moffat.EndlessOnline.SDK.Data;

namespace Moffat.EndlessOnline.SDK.Test.Encrypt;

[TestFixture]
public class ServerVerifierTest
{
    public static TestCaseData[] ServerVerificationHashData => new TestCaseData[]
    {
        new TestCaseData(0).Returns(114000),
        new TestCaseData(1).Returns(115191),
        new TestCaseData(2).Returns(229432),
        new TestCaseData(5).Returns(613210),
        new TestCaseData(12345).Returns(266403),
        new TestCaseData(100_000).Returns(145554),
        new TestCaseData(5_000_000).Returns(339168),
        new TestCaseData(11_092_003).Returns(112773),
        new TestCaseData(11_092_004).Returns(112655),
        new TestCaseData(11_092_005).Returns(112299),
        new TestCaseData(11_092_110).Returns(11016),
        new TestCaseData(11_092_111).Returns(-2787),
        new TestCaseData(11_111_111).Returns(103749),
        new TestCaseData(12_345_678).Returns(-32046),
        new TestCaseData((int)EoNumericLimits.THREE_MAX - 1).Returns(105960),
    };

    [TestCaseSource(nameof(ServerVerificationHashData))]
    public int TestServerVerificationHash(int input)
    {
        return ServerVerifier.Hash(input);
    }
}
