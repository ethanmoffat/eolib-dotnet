using System.Xml.Linq;
using System.Xml.Serialization;
using ProtocolGenerator.Extensions;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Test.Extensions;

public class XElementCommentExtensionsTest
{
    [Test]
    public void RewriteCommentsAsElementsInPlace_CommentBeforeElement_AttachesToNextSibling()
    {
        var spec = Deserialize(@"
<protocol>
    <struct name=""S"">
        <field name=""a"" type=""char""/>
        <!-- about b -->
        <field name=""b"" type=""char""/>
    </struct>
</protocol>");

        var fields = spec.Structs[0].Instructions.Cast<ProtocolFieldInstruction>().ToList();
        Assert.That(fields[0].Comment, Is.Null);
        Assert.That(fields[1].Comment, Is.EqualTo("about b"));
    }

    [Test]
    public void RewriteCommentsAsElementsInPlace_ConsecutiveComments_JoinedInOrder()
    {
        var spec = Deserialize(@"
<protocol>
    <struct name=""S"">
        <!-- first -->
        <!--
            second
            third
        -->
        <dummy type=""byte"">0</dummy>
    </struct>
</protocol>");

        var dummy = (ProtocolDummyInstruction)spec.Structs[0].Instructions[0];
        Assert.That(dummy.Comment, Is.EqualTo("first\nsecond\nthird"));
        Assert.That(dummy.Content, Is.EqualTo("0"));
    }

    [Test]
    public void RewriteCommentsAsElementsInPlace_ExplicitCommentElement_MergedBeforeXmlComment()
    {
        var spec = Deserialize(@"
<protocol>
    <struct name=""S"">
        <!-- xml comment -->
        <field name=""a"" type=""char"">
            <comment>explicit comment</comment>
        </field>
    </struct>
</protocol>");

        var field = (ProtocolFieldInstruction)spec.Structs[0].Instructions[0];
        Assert.That(field.Comment, Is.EqualTo("explicit comment\nxml comment"));
    }

    [Test]
    public void RewriteCommentsAsElementsInPlace_TrailingComment_AttachesToParent()
    {
        var spec = Deserialize(@"
<protocol>
    <packet family=""Account"" action=""Reply"">
        <field name=""reply_code"" type=""char""/>
        <switch field=""reply_code"">
            <case value=""0"">
                <!-- empty case -->
            </case>
        </switch>
    </packet>
</protocol>");

        var @switch = (ProtocolSwitchInstruction)spec.Packets[0].Instructions[1];
        Assert.That(@switch.Comment, Is.Null);
        Assert.That(@switch.Cases[0].Comment, Is.EqualTo("empty case"));
        Assert.That(@switch.Cases[0].Instructions, Is.Empty);
    }

    [Test]
    public void RewriteCommentsAsElementsInPlace_CommentBeforeClosingParent_DoesNotAttachAcrossParentBoundary()
    {
        var spec = Deserialize(@"
<protocol>
    <struct name=""S"">
        <chunked>
            <field name=""a"" type=""char""/>
            <!-- end of chunked -->
        </chunked>
        <field name=""b"" type=""char""/>
    </struct>
</protocol>");

        var chunked = (ProtocolChunkedInstruction)spec.Structs[0].Instructions[0];
        var field = (ProtocolFieldInstruction)spec.Structs[0].Instructions[1];
        Assert.That(chunked.Comment, Is.EqualTo("end of chunked"));
        Assert.That(field.Comment, Is.Null);
    }

    [Test]
    public void RewriteCommentsAsElementsInPlace_CommentBeforeEnumValue_AttachesToValue()
    {
        var spec = Deserialize(@"
<protocol>
    <enum name=""E"" type=""char"">
        <!-- about one -->
        <value name=""One"">1</value>
    </enum>
</protocol>");

        var value = spec.Enums[0].Values[0];
        Assert.That(value.Comment, Is.EqualTo("about one"));
        Assert.That(value.Value, Is.EqualTo("1"));
    }

    [TestCase("<field name=\"a\" type=\"char\"><comment>c</comment>5</field>")]
    [TestCase("<!-- c -->\n<field name=\"a\" type=\"char\">5</field>")]
    public void RewriteCommentsAsElementsInPlace_FieldWithContentAndComment_ContentIsPreserved(string fieldXml)
    {
        var spec = Deserialize($"<protocol><struct name=\"S\">{fieldXml}</struct></protocol>");

        var field = (ProtocolFieldInstruction)spec.Structs[0].Instructions[0];
        Assert.That(field.Comment, Is.EqualTo("c"));
        Assert.That(field.Content, Is.EqualTo("5"));
    }

    private static ProtocolSpec Deserialize(string xml)
    {
        var document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        document.Root!.RewriteCommentsAsElementsInPlace();
        return (ProtocolSpec)new XmlSerializer(typeof(ProtocolSpec)).Deserialize(document.CreateReader())!;
    }
}
