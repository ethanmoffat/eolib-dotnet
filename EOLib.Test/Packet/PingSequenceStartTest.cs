namespace EOLib.Packet;

[TestFixture]
public class PingSequenceStartTest
{
    // note: the expected values differ from eolib-java due to a different pseudo-random implementation
    private const int Expected = 1729;
    private const int Seq1 = 1957;
    private const int Seq2 = 228;

    [Test]
    public void TestFromValue()
    {
        var sequenceStart = PingSequenceStart.FromPingValues(Seq1, Seq2);
        Assert.That(sequenceStart.Value, Is.EqualTo(Expected));
    }

    [Test]
    public void TestGenerate()
    {
        const int Seed = 123;
        var sequenceStart = PingSequenceStart.Generate(new Random(Seed));
        Assert.That(sequenceStart.Value, Is.EqualTo(Expected));
    }
}