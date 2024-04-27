using System.Text;
using System.Xml.Serialization;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Test.Model.Xml;

public class ModelDeserializeTest
{
    private const string EnumXml = @"
<protocol>
    <enum name=""AdminLevel"" type=""char"">
        <comment>The admin level of a player</comment>
        <value name=""Player"" >0</value>
        <value name=""Spy"">1</value>
        <value name=""LightGuide"">2</value>
        <value name=""Guardian"">3</value>
        <value name=""GameMaster"">4</value>
        <value name=""HighGameMaster"">5</value>
    </enum>
</protocol>";

    private const string StructXml = @"
<protocol>
    <struct name=""AvatarChange"">
        <comment>Information about a nearby player's appearance changing</comment>
        <field name=""player_id"" type=""short""/>
        <field name=""change_type"" type=""AvatarChangeType""/>
        <field name=""sound"" type=""bool""/>
        <switch field=""change_type"">
            <case value=""Equipment"">
                <field name=""equipment"" type=""EquipmentChange""/>
            </case>
            <case value=""Hair"">
                <field name=""hair_style"" type=""char""/>
                <field name=""hair_color"" type=""char""/>
            </case>
            <case value=""HairColor"">
                <field name=""hair_color"" type=""char""/>
            </case>
        </switch>
    </struct>

    <struct name=""NearbyInfo"">
        <comment>Information about nearby entities</comment>
        <length name=""characters_count"" type=""char""/>
        <chunked>
            <break/>
            <array name=""characters"" type=""CharacterMapInfo"" length=""characters_count"" delimited=""true""/>
            <array name=""npcs"" type=""NpcMapInfo""/>
            <break/>
            <array name=""items"" type=""ItemMapInfo""/>
        </chunked>
    </struct>
</protocol>";

    private const string PacketXml = @"
<protocol>
    <packet family=""Message"" action=""Close"">
        <comment>Server is rebooting</comment>
        <dummy type=""string"">r</dummy>
    </packet>

    <packet family=""Init"" action=""Init"">
        <comment>
            Reply to connection initialization and requests for unencrypted data.
            This packet is unencrypted.
        </comment>
        <field name=""reply_code"" type=""InitReply""/>
        <switch field=""reply_code"">
            <case value=""OutOfDate"">
                <field name=""version"" type=""Version""/>
            </case>
            <case value=""Ok"">
                <field name=""seq1"" type=""byte""/>
                <field name=""seq2"" type=""byte""/>
                <field name=""server_encryption_multiple"" type=""byte""/>
                <field name=""client_encryption_multiple"" type=""byte""/>
                <field name=""player_id"" type=""short""/>
                <field name=""challenge_response"" type=""three""/>
            </case>
            <case value=""Banned"">
                <field name=""ban_type"" type=""InitBanType""/>
                <switch field=""ban_type"">
                    <case value=""0"">
                        <comment>
                            The official client treats any value below 2 as a temporary ban.
                            The official server sends 1, but some game server implementations
                            erroneously send 0.
                        </comment>
                        <field name=""minutes_remaining"" type=""byte""/>
                    </case>
                    <case value=""Temporary"">
                        <field name=""minutes_remaining"" type=""byte""/>
                    </case>
                </switch>
            </case>
            <case value=""WarpMap"">
                <field name=""map_file"" type=""MapFile""/>
            </case>
            <case value=""FileEmf"">
                <field name=""map_file"" type=""MapFile""/>
            </case>
            <case value=""FileEif"">
                <field name=""pub_file"" type=""PubFile""/>
            </case>
            <case value=""FileEnf"">
                <field name=""pub_file"" type=""PubFile""/>
            </case>
            <case value=""FileEsf"">
                <field name=""pub_file"" type=""PubFile""/>
            </case>
            <case value=""FileEcf"">
                <field name=""pub_file"" type=""PubFile""/>
            </case>
            <case value=""MapMutation"">
                <field name=""map_file"" type=""MapFile""/>
            </case>
            <case value=""PlayersList"">
                <chunked>
                    <field name=""players_list"" type=""PlayersList""/>
                </chunked>
            </case>
            <case value=""PlayersListFriends"">
                <chunked>
                    <field name=""players_list"" type=""PlayersListFriends""/>
                </chunked>
            </case>
        </switch>
    </packet>
</protocol>
    ";

    [Test]
    public void ProtocolSpec_Enums_AreDeserializedCorrectly()
    {
        using var sr = new StringReader(EnumXml);
        var serializer = new XmlSerializer(typeof(ProtocolSpec));
        var model = (ProtocolSpec)serializer.Deserialize(sr)!;

        Assert.That(model.Enums, Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            var e = model.Enums[0];
            Assert.That(e.Name, Is.EqualTo("AdminLevel"));
            Assert.That(e.Type, Is.EqualTo("char"));
            Assert.That(e.Values, Has.Count.EqualTo(6));
            Assert.That(e.Values.Select(x => x.Name), Is.EquivalentTo(new[] { "Player", "Spy", "LightGuide", "Guardian", "GameMaster", "HighGameMaster" }));
            Assert.That(e.Values.Select(x => x.Value), Is.EquivalentTo(new[] { "0", "1", "2", "3", "4", "5" }));
        });
    }

    [Test]
    public void ProtocolSpec_Structs_AreDeserializedCorrectly()
    {
        using var sr = new StringReader(StructXml);
        var serializer = new XmlSerializer(typeof(ProtocolSpec));
        var model = (ProtocolSpec)serializer.Deserialize(sr)!;

        Assert.That(model.Structs, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            var s = model.Structs[0];
            Assert.That(s.Name, Is.EqualTo("AvatarChange"));
            Assert.That(s.Instructions, Has.Count.EqualTo(4));
            Assert.That(
                s.Instructions.Select(x => x.GetType()),
                Is.EqualTo(new[] { typeof(ProtocolFieldInstruction), typeof(ProtocolFieldInstruction), typeof(ProtocolFieldInstruction), typeof(ProtocolSwitchInstruction) })
            );
            Assert.That(((ProtocolSwitchInstruction)s.Instructions[3]).Field, Is.EqualTo("change_type"));
            Assert.That(((ProtocolSwitchInstruction)s.Instructions[3]).Cases, Has.Count.EqualTo(3));
        });

        Assert.Multiple(() =>
        {
            var s = model.Structs[1];
            Assert.That(s.Name, Is.EqualTo("NearbyInfo"));
            Assert.That(s.Instructions, Has.Count.EqualTo(2));
            Assert.That(
                s.Instructions.Select(x => x.GetType()),
                Is.EqualTo(new[] { typeof(ProtocolLengthInstruction), typeof(ProtocolChunkedInstruction) })
            );
            Assert.That(
                ((ProtocolChunkedInstruction)s.Instructions[1]).Instructions.Select(x => x?.GetType()),
                Is.EqualTo(new[] { typeof(ProtocolBreakInstruction), typeof(ProtocolArrayInstruction), typeof(ProtocolArrayInstruction), typeof(ProtocolBreakInstruction), typeof(ProtocolArrayInstruction) })
            );
        });
    }

    [Test]
    public void ProtocolSpec_Packets_AreDeserializedCorrectly()
    {
        using var sr = new StringReader(PacketXml);
        var serializer = new XmlSerializer(typeof(ProtocolSpec));
        var model = (ProtocolSpec)serializer.Deserialize(sr)!;

        Assert.That(model.Packets, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            var p = model.Packets[0];
            Assert.That(p.Family, Is.EqualTo("Message"));
            Assert.That(p.Action, Is.EqualTo("Close"));
            Assert.That(p.Instructions, Has.Count.EqualTo(1));
            Assert.That(
                p.Instructions.Select(x => x.GetType()),
                Is.EqualTo(new[] { typeof(ProtocolDummyInstruction) })
            );
            Assert.That(((ProtocolDummyInstruction)p.Instructions[0]).Type, Is.EqualTo("string"));
            Assert.That(((ProtocolDummyInstruction)p.Instructions[0]).Content, Is.EqualTo("r"));
        });

        Assert.Multiple(() =>
        {
            var p = model.Packets[1];
            Assert.That(p.Family, Is.EqualTo("Init"));
            Assert.That(p.Action, Is.EqualTo("Init"));
            Assert.That(p.Instructions, Has.Count.EqualTo(2));
            Assert.That(
                p.Instructions.Select(x => x.GetType()),
                Is.EqualTo(new[] { typeof(ProtocolFieldInstruction), typeof(ProtocolSwitchInstruction) })
            );
            Assert.That(((ProtocolSwitchInstruction)p.Instructions[1]).Cases, Has.Count.EqualTo(12));
        });
    }
}
