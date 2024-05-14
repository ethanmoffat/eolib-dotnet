namespace Moffat.EndlessOnline.SDK.Packet;

/// <summary>
/// A class representing the sequence start value sent with the ACCOUNT_REPLY server packet.
/// </summary>
public class AccountReplySequenceStart : ISequenceStart
{
    /// <inheritdoc/>
    public int Value { get; }

    private AccountReplySequenceStart(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates an instance of <see cref="AccountReplySequenceStart"/> from the value sent with the ACCOUNT_REPLY server packet.
    /// </summary>
    /// <remarks>
    /// See <see cref="AccountReplyServerPacket.ReplyCodeDataDefault.SequenceStart" />.
    /// </remarks>
    /// <param name="value">The sequence_start char value sent with the ACCOUNT_REPLY server packet</param>
    /// <returns>An instance of <see cref="AccountReplySequenceStart"/></returns>
    public static AccountReplySequenceStart FromValue(int value)
    {
        return new AccountReplySequenceStart(value);
    }

    /// <summary>
    /// Generates an instance of <see cref="AccountReplySequenceStart"/> with a random value in the range <c>0-240</c>.
    /// </summary>
    /// <param name="random">The random number generator to use</param>
    /// <returns>An instance of <see cref="AccountReplySequenceStart"/></returns>
    public static AccountReplySequenceStart Generate(Random random)
    {
        int start = random.Next(240);
        return new AccountReplySequenceStart(start);
    }
}