using EOLib.Data;

namespace EOLib.Packet;

public sealed class InitSequenceStart : ISequenceStart
{
    /// <inheritdoc />
    public int Value { get; }

    /// <summary>
    /// Gets the seq1 byte value sent with the INIT_INIT server packet. See <see cref="InitInitServerPacket.ReplyCodeDataOk.Seq1" />.
    /// </summary>
    public int Seq1 { get; }

    /// <summary>
    /// Gets the seq2 byte value sent with the INIT_INIT server packet. See <see cref="InitInitServerPacket.ReplyCodeDataOk.Seq2" />.
    /// </summary>
    public int Seq2 { get; }

    private InitSequenceStart(int value, int seq1, int seq2)
    {
        Value = value;
        Seq1 = seq1;
        Seq2 = seq2;
    }

    /// <summary>
    /// Creates an instance of <see cref="InitSequenceStart"/> from the values sent with the INIT_INIT server packet.
    /// </summary>
    /// <remarks>
    /// See <see cref="InitInitServerPacket.ReplyCodeDataOk.Seq1" />. See also <seealso cref="InitInitServerPacket.ReplyCodeDataOk.Seq2" />.
    /// </remarks>
    /// <param name="seq1">The seq1 byte value sent with the INIT_INIT server packet</param>
    /// <param name="seq2">The seq2 byte value sent with the INIT_INIT server packet</param>
    /// <returns>An instance of <see cref="InitSequenceStart"/></returns>
    public static InitSequenceStart FromInitValues(int seq1, int seq2)
    {
        int value = seq1 * 7 + seq2 - 13;
        return new InitSequenceStart(value, seq1, seq2);
    }

    /// <summary>
    /// Generates an instance of <see cref="InitSequenceStart"/> with a random value in the range <c>0-1757</c>.
    /// </summary>
    /// <param name="random">The random number generator to use</param>
    /// <returns>An instance of <see cref="InitSequenceStart"/></returns>
    public static InitSequenceStart Generate(Random random)
    {
        int value = random.Next(1757);
        int seq1Max = (value + 13) / 7;
        int seq1Min = Math.Max(0, (value - (EoNumericLimits.CHAR_MAX - 1) + 13 + 6) / 7);

        int seq1 = random.Next(seq1Max - seq1Min) + seq1Min;
        int seq2 = value - seq1 * 7 + 13;

        return new InitSequenceStart(value, seq1, seq2);
    }
}