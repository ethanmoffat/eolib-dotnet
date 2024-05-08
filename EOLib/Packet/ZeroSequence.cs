namespace EOLib.Packet;

/// <summary>
/// A sequence start with a fixed value of zero.
/// </summary>
public class ZeroSequence : ISequenceStart
{
    /// <summary>
    /// Gets an instance of <see cref="ISequenceStart" /> with a value of <c>0</c>
    /// </summary>
    public static ISequenceStart Instance { get; } = new ZeroSequence();

    private ZeroSequence() { }

    /// <inheritdoc />
    public int Value => 0;
}
