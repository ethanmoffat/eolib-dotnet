namespace EOLib.Packet;

[TestFixture]
public class ZeroSequenceTest
{
    [Test]
    public void TestZeroSequenceHasZeroValue()
    {
        Assert.That(ZeroSequence.Instance.Value, Is.Zero);
    }

    [Test]
    public void TestZeroIsZeroSequence()
    {
        Assert.That(ZeroSequence.Instance, Is.InstanceOf<ZeroSequence>());
    }
}
