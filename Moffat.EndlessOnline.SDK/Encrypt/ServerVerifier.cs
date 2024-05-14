namespace Moffat.EndlessOnline.SDK.Data;

/// <summary>
/// A helper class for verifying that the game server is genuine.
/// </summary>
public static class ServerVerifier
{
    /// <summary>
    /// This hash function is how the game client checks that it's communicating with a genuine server during connection initialization.
    /// <para>- The client sends an integer value to the server in the INIT_INIT client packet, where it is referred to as the <c>challenge</c>.</para>
    /// <para>- The server hashes the value and sends the hash back in the INIT_INIT server packet.</para>
    /// <para>- The client hashes the value and compares it to the hash sent by the server.</para>
    /// <para>- If the hashes don't match, the client drops the connection.</para>
    /// </summary>
    /// <remarks>
    /// WARNING: Oversized challenges may result in negative hash values, which cannot be represented properly in the EO protocol.
    /// <para>See: <see cref="InitInitClientPacket.Challenge" /></para>
    /// <para>See also: <seealso cref="InitInitServerPacket.ReplyCodeDataOk.ChallengeResponse" /></para>
    /// </remarks>
    /// <param name="challenge">The challenge value sent by the client. Should be no larger than <c>11,092,110</c>.</param>
    /// <returns>The hashed challenge value.</returns>
    public static int Hash(int challenge)
    {
        ++challenge;
        return 110905
            + (challenge % 9 + 1) * ((11092004 - challenge) % ((challenge % 11 + 1) * 119)) * 119
            + challenge % 2004;
    }
}
