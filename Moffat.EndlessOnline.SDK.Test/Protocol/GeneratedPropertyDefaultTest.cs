using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace Moffat.EndlessOnline.SDK.Test.Protocol;

[TestFixture]
public class GeneratedPropertyDefaultTest
{
    [Test]
    public void NewObject_StringField_DefaultsToEmptyString()
    {
        var packet = new InitInitClientPacket();

        Assert.That(packet.Hdid, Is.EqualTo(string.Empty));
    }
}
