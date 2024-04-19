namespace EOLib.Packet;

/// <summary>
/// A value sent by the server to update the client's sequence start, also known as the "starting counter ID".
/// </summary>
public interface ISequenceStart
{
    /// <summary>
    /// Gets the sequence start value
    /// </summary>
    int Value { get; }

    /// <summary>
    /// Gets an instance of <see cref="ISequenceStart" /> with a value of <c>0</c>
    /// </summary>
    static ISequenceStart Zero { get; } = new ZeroSequence();
}