namespace Moffat.EndlessOnline.SDK.Packet;

/// <summary>
/// A class for generating packet sequences
/// </summary>
public sealed class PacketSequencer
{
    private ISequenceStart _start;
    private int _counter;

    /// <summary>
    /// Constructs a new PacketSequencer with the provided <see cref="SequenceStart" />
    /// </summary>
    /// <param name="start">The sequence start</param>
    public PacketSequencer(ISequenceStart start)
    {
        _start = start;
    }

    private PacketSequencer(ISequenceStart start, int counter)
    {
        _start = start;
        _counter = counter;
    }

    /// <summary>
    /// Calculates and returns the next sequence value, updating the sequence counter in the process
    /// </summary>
    /// <remarks>
    /// Note: this is not a monotonic operation. The sequence counter increases from 0 to 9 before looping back to 0
    /// </remarks>
    /// <returns>The next sequence value based on the SequenceStart value and the current counter</returns>
    public int NextSequence()
    {
        int result = _start.Value + _counter;
        _counter = (_counter + 1) % 10;
        return result;
    }

    /// <summary>
    /// Creates a new PacketSequencer with an updated sequence start, also known as the "starting counter ID"
    /// </summary>
    /// <remarks>
    /// Note: this does not reset the sequence counter
    /// </remarks>
    /// <param name="start">The new sequence start</param>
    /// <returns>A new packet sequencer instance with this sequencer's counter value and an updated sequence start value</returns>
    public PacketSequencer WithSequenceStart(ISequenceStart start)
    {
        return new PacketSequencer(start, _counter);
    }
}