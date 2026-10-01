using Microsoft.CodeAnalysis;
using static ProtocolGenerator.Test.GeneratorTestHarness;

namespace ProtocolGenerator.Test;

public class ProtocolCommentGenerationTest
{
    [Test]
    public void Generate_CommentOnNamedField_EmittedOnProperty()
    {
        var source = Generate(@"
<struct name=""S"">
    <!-- about a -->
    <field name=""a"" type=""char""/>
</struct>");

        Assert.That(source, Does.Contain(Doc("    ", "about a") + "\n    public int A { get; set; }"));
    }

    [Test]
    public void Generate_CommentOnUnnamedInstructions_EmittedAsTypeRemarks()
    {
        var source = Generate(@"
<struct name=""S"">
    <comment>The struct</comment>
    <!-- about the unnamed field -->
    <field type=""char"">1</field>
    <chunked>
        <field name=""a"" type=""char""/>
        <!-- about the break -->
        <break/>
        <!-- about the dummy -->
        <dummy type=""byte"">0</dummy>
    </chunked>
</struct>");

        Assert.That(source, Does.Contain(
            "/// <summary>\n/// The struct\n/// </summary>\n" +
            "/// <remarks>\n" +
            "/// <para>\n/// about the unnamed field\n/// </para>\n" +
            "/// <para>\n/// about the break\n/// </para>\n" +
            "/// <para>\n/// about the dummy\n/// </para>\n" +
            "/// </remarks>\n" +
            "[Generated]\npublic class S"));
        Assert.That(source, Does.Not.Contain("        // about"));
    }

    [Test]
    public void Generate_CommentOnEnumValue_EmittedOnValue()
    {
        var source = Generate(@"
<enum name=""E"" type=""char"">
    <!-- about one -->
    <value name=""One"">1</value>
</enum>");

        Assert.That(source, Does.Contain(Doc("    ", "about one") + "\n    One = 1,"));
    }

    [Test]
    public void Generate_CommentInEmptyCases_EmittedOnSwitchPropertyAndNoCaseLabels()
    {
        var source = Generate(@"
<packet family=""Account"" action=""Reply"">
    <field name=""reply_code"" type=""short""/>
    <switch field=""reply_code"">
        <case value=""0"">
            <!-- no effect -->
        </case>
        <case value=""1"">
            <field name=""a"" type=""char""/>
        </case>
        <case value=""2"">
            <!-- no effect -->
        </case>
        <case value=""3"">
            <!-- something else -->
        </case>
        <case value=""4""/>
        <case default=""true"">
            <!-- no effect -->
        </case>
    </switch>
</packet>");

        Assert.That(source, Does.Contain(
            Doc("    ", "When ReplyCode is 0, 2 or any other value: no effect", "When ReplyCode is 3: something else") +
            "\n    public IReplyCodeData ReplyCodeData { get; set; }"));
        Assert.That(source, Does.Contain("case (int)1:"));
        Assert.That(source, Does.Not.Contain("case (int)0:"));
        Assert.That(source, Does.Not.Contain("case (int)2:"));
        Assert.That(source, Does.Not.Contain("case (int)3:"));
    }

    [Test]
    public void Generate_CommentInCaseWithData_EmittedOnCaseType()
    {
        var source = Generate(@"
<packet family=""Account"" action=""Reply"">
    <field name=""reply_code"" type=""short""/>
    <switch field=""reply_code"">
        <case value=""1"">
            <comment>Case one</comment>
            <field name=""a"" type=""char""/>
        </case>
    </switch>
</packet>");

        Assert.That(source, Does.Contain(Doc("    ", "Case one") + "\n    [Generated]\n    public class ReplyCodeData1"));
    }

    [Test]
    public void Generate_NamedFixedFieldWithComment_ConstantAndPropertyDocumented()
    {
        var source = Generate(@"
<packet family=""Init"" action=""Init"">
    <!-- must be 112 -->
    <field name=""protocol_version"" type=""char"">112</field>
</packet>");

        Assert.That(source, Does.Contain(
            Doc("    ", "must be 112") + "\n    public const int DefaultProtocolVersion = 112;\n\n" +
            Doc("    ", "must be 112") + "\n    public int ProtocolVersion { get; private set; } = DefaultProtocolVersion;"));
        Assert.That(source, Does.Contain("ProtocolVersion = reader.GetChar();"));
        Assert.That(source, Does.Contain("writer.AddChar(ProtocolVersion);"));
    }

    [Test]
    public void Generate_NamedFixedFieldWithoutComment_ConstantDocumentedWithTemplate()
    {
        var source = Generate(@"
<packet family=""Init"" action=""Init"">
    <field name=""request_string"" type=""string"">NEW</field>
</packet>");

        Assert.That(source, Does.Contain(
            Doc("    ", "The default value of the `request_string` field.") + "\n    public const string DefaultRequestString = \"NEW\";\n\n" +
            "    public string RequestString { get; private set; } = DefaultRequestString;"));
        Assert.That(source, Does.Contain("RequestString = reader.GetString();"));
    }

    [Test]
    public void Generate_NamedStringField_DefaultsToEmptyString()
    {
        var source = Generate(@"
<struct name=""S"">
    <field name=""a"" type=""string""/>
</struct>");

        Assert.That(source, Does.Contain("public string A { get; set; } = \"\";"));
    }

    private static string Generate(string body)
    {
        var result = Run(new ProtocolIncrementalGenerator(), ("xml/protocol.xml", ProtocolXml(body)));
        Assert.That(result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error), Is.Empty);
        return result.GeneratedTrees.Last().ToString().Replace("\r\n", "\n");
    }

    private static string Doc(string indent, params string[] lines)
    {
        var body = string.Concat(lines.Select(x => $"{indent}/// {x}\n"));
        return $"{indent}/// <summary>\n{body}{indent}/// </summary>";
    }
}
