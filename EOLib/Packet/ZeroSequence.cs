namespace EOLib.Packet;

/// <summary>
/// A sequence start with a fixed value of zero.
/// </summary>
public class ZeroSequence : ISequenceStart
{
    /// <inheritdoc />
    public int Value => 0;
}