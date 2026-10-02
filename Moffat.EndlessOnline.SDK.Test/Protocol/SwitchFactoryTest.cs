using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace Moffat.EndlessOnline.SDK.Test.Protocol;

[TestFixture]
public class SwitchFactoryTest
{
    [Test]
    public void LoginReplyForOk_RoundTrip_SetsCodeAndData()
    {
        var data = new LoginReplyServerPacket.ReplyCodeDataOk
        {
            Characters = new List<CharacterSelectionListEntry>
            {
                new() { Name = "hello", Id = 7, Level = 3, Equipment = new EquipmentCharacterSelect() },
            },
        };

        var packet = LoginReplyServerPacket.ForOk(data);
        var actual = RoundTrip(packet);

        Assert.That(packet.ReplyCode, Is.EqualTo(LoginReply.Ok));
        Assert.That(packet.ReplyCodeData, Is.SameAs(data));
        Assert.That(actual.ReplyCode, Is.EqualTo(LoginReply.Ok));
        Assert.That(actual.ReplyCodeData, Is.InstanceOf<LoginReplyServerPacket.ReplyCodeDataOk>());
        Assert.That(actual, Is.EqualTo(packet));
    }

    [Test]
    public void LoginReplyForOk_NullData_ThrowsArgumentNullException()
    {
        Assert.That(() => LoginReplyServerPacket.ForOk(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("data"));
    }

    [Test]
    public void LoginReplyForWrongUser_RoundTrip_CreatesHardcodedData()
    {
        var packet = LoginReplyServerPacket.ForWrongUser();
        var actual = RoundTrip(packet);

        Assert.That(packet.ReplyCodeData, Is.InstanceOf<LoginReplyServerPacket.ReplyCodeDataWrongUser>());
        Assert.That(actual.ReplyCode, Is.EqualTo(LoginReply.WrongUser));
        Assert.That(actual.ReplyCodeData, Is.InstanceOf<LoginReplyServerPacket.ReplyCodeDataWrongUser>());
        Assert.That(actual, Is.EqualTo(packet));
    }

    [Test]
    public void InitInitForBannedTemporary_RoundTrip_SetsBothCodesAndData()
    {
        var packet = InitInitServerPacket.ForBannedTemporary(
            new InitInitServerPacket.ReplyCodeDataBanned.BanTypeDataTemporary { MinutesRemaining = 30 });
        var actual = RoundTrip(packet);

        Assert.That(actual.ReplyCode, Is.EqualTo(InitReply.Banned));
        var banned = actual.ReplyCodeData as InitInitServerPacket.ReplyCodeDataBanned;
        Assert.That(banned, Is.Not.Null);
        Assert.That(banned!.BanType, Is.EqualTo(InitBanType.Temporary));
        Assert.That(banned.BanTypeData, Is.InstanceOf<InitInitServerPacket.ReplyCodeDataBanned.BanTypeDataTemporary>());
        Assert.That(((InitInitServerPacket.ReplyCodeDataBanned.BanTypeDataTemporary)banned.BanTypeData).MinutesRemaining, Is.EqualTo(30));
        Assert.That(actual, Is.EqualTo(packet));
    }

    [Test]
    public void InitInitForBannedPermanent_RoundTrip_SetsBothCodesWithoutData()
    {
        var packet = InitInitServerPacket.ForBannedPermanent();
        var actual = RoundTrip(packet);

        Assert.That(actual.ReplyCode, Is.EqualTo(InitReply.Banned));
        var banned = actual.ReplyCodeData as InitInitServerPacket.ReplyCodeDataBanned;
        Assert.That(banned, Is.Not.Null);
        Assert.That(banned!.BanType, Is.EqualTo(InitBanType.Permanent));
        Assert.That(banned.BanTypeData, Is.Null);
        Assert.That(actual, Is.EqualTo(packet));
    }

    [Test]
    public void InitInitForBanTypeData0_RoundTrip_SetsNumericCodeAndData()
    {
        var packet = InitInitServerPacket.ForBanTypeData0(
            new InitInitServerPacket.ReplyCodeDataBanned.BanTypeData0 { MinutesRemaining = 5 });
        var actual = RoundTrip(packet);

        Assert.That(actual.ReplyCode, Is.EqualTo(InitReply.Banned));
        var banned = actual.ReplyCodeData as InitInitServerPacket.ReplyCodeDataBanned;
        Assert.That(banned, Is.Not.Null);
        Assert.That(banned!.BanType, Is.EqualTo((InitBanType)0));
        Assert.That(banned.BanTypeData, Is.InstanceOf<InitInitServerPacket.ReplyCodeDataBanned.BanTypeData0>());
        Assert.That(actual, Is.EqualTo(packet));
    }

    [Test]
    public void AccountReplyForReplyCodeDefault_RoundTrip_SetsCodeAndData()
    {
        const AccountReply SessionId = (AccountReply)1234;

        var packet = AccountReplyServerPacket.ForReplyCodeDefault(
            SessionId, new AccountReplyServerPacket.ReplyCodeDataDefault { SequenceStart = 42 });
        var actual = RoundTrip(packet);

        Assert.That(actual.ReplyCode, Is.EqualTo(SessionId));
        Assert.That(actual.ReplyCodeData, Is.InstanceOf<AccountReplyServerPacket.ReplyCodeDataDefault>());
        Assert.That(((AccountReplyServerPacket.ReplyCodeDataDefault)actual.ReplyCodeData).SequenceStart, Is.EqualTo(42));
        Assert.That(actual, Is.EqualTo(packet));
    }

    [TestCase(AccountReply.Exists)]
    [TestCase(AccountReply.RequestDenied)]
    [TestCase((AccountReply)0)]
    [TestCase((AccountReply)9)]
    public void AccountReplyForReplyCodeDefault_CodeWithOwnCase_ThrowsArgumentException(AccountReply code)
    {
        var data = new AccountReplyServerPacket.ReplyCodeDataDefault();

        Assert.That(() => AccountReplyServerPacket.ForReplyCodeDefault(code, data),
            Throws.ArgumentException
                .With.Property("ParamName").EqualTo("code")
                .And.Message.Contains(code.ToString()));
    }

    [Test]
    public void AccountReplyForReplyCodeDefault_NullData_ThrowsArgumentNullException()
    {
        Assert.That(() => AccountReplyServerPacket.ForReplyCodeDefault((AccountReply)10, null!),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("data"));
    }

    [Test]
    public void DialogEntryForLink_RoundTrip_SetsCodeAndData()
    {
        var entry = DialogEntry.ForLink(new DialogEntry.EntryTypeDataLink { LinkId = 3 });
        entry.Line = "go";
        var actual = RoundTrip(entry);

        Assert.That(actual.EntryType, Is.EqualTo(DialogEntryType.Link));
        Assert.That(actual.EntryTypeData, Is.InstanceOf<DialogEntry.EntryTypeDataLink>());
        Assert.That(((DialogEntry.EntryTypeDataLink)actual.EntryTypeData).LinkId, Is.EqualTo(3));
        Assert.That(actual, Is.EqualTo(entry));
    }

    [Test]
    public void DialogEntryForText_RoundTrip_SetsCodeWithoutData()
    {
        var entry = DialogEntry.ForText();
        entry.Line = "hi";
        var actual = RoundTrip(entry);

        Assert.That(actual.EntryType, Is.EqualTo(DialogEntryType.Text));
        Assert.That(actual.EntryTypeData, Is.Null);
        Assert.That(actual, Is.EqualTo(entry));
    }

    private static T RoundTrip<T>(T value) where T : ISerializable, new()
    {
        var writer = new EoWriter();
        value.Serialize(writer);

        var actual = new T();
        actual.Deserialize(new EoReader(writer.ToByteArray()));
        return actual;
    }
}
