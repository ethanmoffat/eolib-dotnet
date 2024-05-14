namespace Moffat.EndlessOnline.SDK.Packet;

[TestFixture]
public class InitSequenceStartTest
{
    // note: the expected values differ from eolib-java due to a different pseudo-random implementation
    private const int Expected = 1729;
    private const int Seq1 = 244;
    private const int Seq2 = 34;

    [Test]
    public void TestFromValue()
    {
        var sequenceStart = InitSequenceStart.FromInitValues(Seq1, Seq2);
        Assert.That(sequenceStart.Value, Is.EqualTo(Expected));
    }

    [Test]
    public void TestGenerate()
    {
        const int Seed = 123;
        var sequenceStart = InitSequenceStart.Generate(new Random(Seed));
        Assert.That(sequenceStart.Value, Is.EqualTo(Expected));
    }
}