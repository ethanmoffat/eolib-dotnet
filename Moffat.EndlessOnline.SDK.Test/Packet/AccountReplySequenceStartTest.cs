namespace Moffat.EndlessOnline.SDK.Packet;

[TestFixture]
public class AccountReplySequenceStartTest
{
    [Test]
    public void TestFromValue()
    {
        const int Expected = 42;
        var sequenceStart = AccountReplySequenceStart.FromValue(Expected);
        Assert.That(sequenceStart.Value, Is.EqualTo(Expected));
    }

    [Test]
    public void TestGenerate()
    {
        const int Seed = 123;
        var sequenceStart = AccountReplySequenceStart.Generate(new Random(Seed));

        // note: the expected values differ from eolib-java due to a different pseudo-random implementation
        Assert.That(sequenceStart.Value, Is.EqualTo(236));
    }
}