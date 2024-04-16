using EOLib.Data;

namespace EOLib.Packet;

/// <summary>
/// A class representing the sequence start value sent with the CONNECTION_PLAYER server packet.
/// </summary>
public class PingSequenceStart : ISequenceStart
{
    /// <inheritdoc />
    public int Value { get; }


    /// <summary>
    /// Gets the seq1 byte value sent with the CONNECTION_PLAYER server packet. See <see cref="ConnectionPlayerServerPacket.Seq1" />.
    /// </summary>
    public int Seq1 { get; }

    /// <summary>
    /// Gets the seq2 byte value sent with the CONNECTION_PLAYER server packet. See <see cref="ConnectionPlayerServerPacket.Seq2" />.
    /// </summary>
    public int Seq2 { get; }

    private PingSequenceStart(int value, int seq1, int seq2)
    {
        Value = value;
        Seq1 = seq1;
        Seq2 = seq2;
    }

    /// <summary>
    /// Creates an instance of <see cref="PingSequenceStart"/> from the values sent with the CONNECTION_PLAYER server packet.
    /// </summary>
    /// <remarks>
    /// See <see cref="ConnectionPlayerServerPacket.Seq1" />. See also <seealso cref="ConnectionPlayerServerPacket.Seq2" />.
    /// </remarks>
    /// <param name="seq1">The seq1 byte value sent with the CONNECTION_PLAYER server packet</param>
    /// <param name="seq2">The seq2 byte value sent with the CONNECTION_PLAYER server packet</param>
    /// <returns>An instance of <see cref="PingSequenceStart"/></returns>
    public static PingSequenceStart FromPingValues(int seq1, int seq2)
    {
        int value = seq1 - seq2;
        return new PingSequenceStart(value, seq1, seq2);
    }

    /// <summary>
    /// Generates an instance of <see cref="PingSequenceStart"/> with a random value in the range <c>0-1757</c>.
    /// </summary>
    /// <param name="random">The random number generator to use</param>
    /// <returns>An instance of <see cref="PingSequenceStart"/></returns>
    public static PingSequenceStart Generate(Random random)
    {
        int value = random.Next(1757);

        int seq1 = value + random.Next(EoNumericLimits.CHAR_MAX - 1);
        int seq2 = seq1 - value;

        return new PingSequenceStart(value, seq1, seq2);
    }
}
