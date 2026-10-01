using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Version = Moffat.EndlessOnline.SDK.Protocol.Net.Version;

namespace Moffat.EndlessOnline.SDK.Test.Protocol;

[TestFixture]
public class NamedFixedFieldTest
{
    [Test]
    public void Deserialize_NonDefaultProtocolVersion_StoresReadValue()
    {
        const int MismatchedProtocolVersion = InitInitClientPacket.DefaultProtocolVersion + 1;
        var expectedBytes = CreateInitInitBytes(MismatchedProtocolVersion);
        var packet = new InitInitClientPacket();

        packet.Deserialize(new EoReader(expectedBytes));

        Assert.That(packet.ProtocolVersion, Is.EqualTo(MismatchedProtocolVersion));

        var writer = new EoWriter();
        packet.Serialize(writer);
        Assert.That(writer.ToByteArray(), Is.EqualTo(expectedBytes));
    }

    [Test]
    public void Serialize_NewObject_WritesDefaultProtocolVersion()
    {
        var packet = new InitInitClientPacket();
        var writer = new EoWriter();

        packet.Serialize(writer);

        var actual = new InitInitClientPacket();
        actual.Deserialize(new EoReader(writer.ToByteArray()));
        Assert.That(actual.ProtocolVersion, Is.EqualTo(InitInitClientPacket.DefaultProtocolVersion));
    }

    [Test]
    public void DefaultProtocolVersion_MatchesSpec_Is112()
    {
        Assert.That(InitInitClientPacket.DefaultProtocolVersion, Is.EqualTo(112));
    }

    private static byte[] CreateInitInitBytes(int protocolVersion)
    {
        var writer = new EoWriter();
        writer.AddThree(0);
        new Version().Serialize(writer);
        writer.AddChar(protocolVersion);
        writer.AddChar(0);
        writer.AddString(string.Empty);
        return writer.ToByteArray();
    }
}
