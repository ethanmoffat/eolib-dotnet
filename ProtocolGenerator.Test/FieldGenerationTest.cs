using Microsoft.CodeAnalysis;
using static ProtocolGenerator.Test.GeneratorTestHarness;

namespace ProtocolGenerator.Test;

public class FieldGenerationTest
{
    [Test]
    public void Generate_OptionalString_ChecksForNullWithoutValueAccess()
    {
        var source = Generate(@"
<struct name=""S"">
    <field name=""a"" type=""string"" optional=""true""/>
</struct>");

        Assert.That(source, Does.Contain("public string A { get; set; }\n"));
        Assert.That(source, Does.Contain("if (A != null)"));
        Assert.That(source, Does.Contain("writer.AddString(A);"));
        Assert.That(source, Does.Not.Contain("A.HasValue"));
        Assert.That(source, Does.Not.Contain("A.Value"));
    }

    [Test]
    public void Generate_OptionalChar_ChecksHasValueAndAccessesValue()
    {
        var source = Generate(@"
<struct name=""S"">
    <field name=""a"" type=""char"" optional=""true""/>
</struct>");

        Assert.That(source, Does.Contain("if (A.HasValue)"));
        Assert.That(source, Does.Contain("writer.AddChar(A.Value);"));
    }

    [Test]
    public void Generate_StringFieldWithChildComment_HasNoDefaultValue()
    {
        var source = Generate(@"
<struct name=""S"">
    <field name=""a"" type=""string"">
        <comment>about a</comment>
    </field>
</struct>");

        Assert.That(source, Does.Contain("public string A { get; set; } = \"\";"));
        Assert.That(source, Does.Not.Contain("DefaultA"));
    }

    [Test]
    public void Generate_OptionalStringFieldWithChildComment_HasNoDefaultValue()
    {
        var source = Generate(@"
<struct name=""S"">
    <field name=""a"" type=""string"" optional=""true"">
        <comment>about a</comment>
    </field>
</struct>");

        Assert.That(source, Does.Contain("public string A { get; set; }\n"));
    }

    private static string Generate(string body)
    {
        var result = Run(new ProtocolIncrementalGenerator(), ("xml/protocol.xml", ProtocolXml(body)));
        Assert.That(result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error), Is.Empty);
        return result.GeneratedTrees.Last().ToString().Replace("\r\n", "\n");
    }
}
