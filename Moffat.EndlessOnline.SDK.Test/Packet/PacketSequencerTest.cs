namespace Moffat.EndlessOnline.SDK.Packet;

[TestFixture]
public class PacketSequencerTest
{
    [Test]
    public void TestNextSequence_LoopsOverValuesZeroToNine_GreaterThanStartValue()
    {
        const int StartValue = 123;
        var sequenceStart = AccountReplySequenceStart.FromValue(StartValue);
        var sequencer = new PacketSequencer(sequenceStart);

        for (int i = 0; i < 10; i++)
        {
            Assert.That(sequencer.NextSequence(), Is.EqualTo(StartValue + i));
        }

        Assert.That(sequencer.NextSequence(), Is.EqualTo(StartValue));
    }

    [Test]
    public void TestSetSequenceStart_ResetsSequenceStartValue()
    {
        const int StartValue = 100;
        const int NewValue = 200;

        var sequenceStart = AccountReplySequenceStart.FromValue(StartValue);
        var sequencer = new PacketSequencer(sequenceStart);

        Assert.That(sequencer.NextSequence(), Is.EqualTo(StartValue));

        sequenceStart = AccountReplySequenceStart.FromValue(NewValue);
        var newSequencer = sequencer.WithSequenceStart(sequenceStart);

        Assert.That(sequencer.NextSequence(), Is.EqualTo(StartValue + 1));
        Assert.That(newSequencer.NextSequence(), Is.EqualTo(NewValue + 1));
    }
}