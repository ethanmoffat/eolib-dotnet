namespace EOLib.Packet;

[TestFixture]
public class ZeroSequenceTest
{
    [Test]
    public void TestZeroSequenceHasZeroValue()
    {
        var sequenceStart = new ZeroSequence();
        Assert.That(sequenceStart.Value, Is.Zero);
    }

    [Test]
    public void TestZeroIsZeroSequence()
    {
        Assert.That(ISequenceStart.Zero, Is.InstanceOf<ZeroSequence>());
    }
}