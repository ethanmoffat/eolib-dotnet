using Microsoft.CodeAnalysis;
using static ProtocolGenerator.Test.GeneratorTestHarness;

namespace ProtocolGenerator.Test;

public class SwitchFactoryGenerationTest
{
    private const string Enums = @"
<enum name=""Code"" type=""char"">
    <value name=""Data"">1</value>
    <value name=""Empty"">2</value>
    <value name=""Missing"">3</value>
    <value name=""Hardcoded"">4</value>
    <value name=""Nested"">5</value>
</enum>
<enum name=""Inner"" type=""char"">
    <value name=""First"">1</value>
    <value name=""Second"">2</value>
    <value name=""Third"">3</value>
</enum>";

    [Test]
    public void Generate_EnumValueWithData_FactoryTakesData()
    {
        var source = Generate(Packet(@"<case value=""Data""><field name=""a"" type=""char""/></case>"));

        Assert.That(source, Does.Contain(
            "    public static TestReplyServerPacket ForData(ReplyCodeDataData data)\n" +
            "    {\n" +
            "        if (data == null)\n" +
            "        {\n" +
            "            throw new ArgumentNullException(nameof(data));\n" +
            "        }\n" +
            "\n" +
            "        return new TestReplyServerPacket\n" +
            "        {\n" +
            "            ReplyCode = Code.Data,\n" +
            "            ReplyCodeData = data,\n" +
            "        };\n" +
            "    }\n"));
    }

    [Test]
    public void Generate_EnumValueWithData_FactoryDocumented()
    {
        var source = Generate(Packet(@"<case value=""Data""><field name=""a"" type=""char""/></case>"));

        Assert.That(source, Does.Contain(
            "    /// <summary>\n" +
            "    /// Creates a new <see cref=\"TestReplyServerPacket\"/> with <see cref=\"ReplyCode\"/> set to <see cref=\"Code.Data\"/> and <see cref=\"ReplyCodeData\"/> set to <paramref name=\"data\"/>.\n" +
            "    /// </summary>\n" +
            "    /// <param name=\"data\">The data for the case.</param>\n" +
            "    /// <returns>The new <see cref=\"TestReplyServerPacket\"/>.</returns>\n" +
            "    /// <exception cref=\"ArgumentNullException\">Thrown when <paramref name=\"data\"/> is null.</exception>\n" +
            "    public static TestReplyServerPacket ForData("));
    }

    [Test]
    public void Generate_EnumValueWithEmptyCase_FactoryHasNoParametersAndNoData()
    {
        var source = Generate(Packet(@"<case value=""Empty""/>"));

        Assert.That(source, Does.Contain(
            "    public static TestReplyServerPacket ForEmpty()\n" +
            "    {\n" +
            "        return new TestReplyServerPacket\n" +
            "        {\n" +
            "            ReplyCode = Code.Empty,\n" +
            "        };\n" +
            "    }\n"));
    }

    [Test]
    public void Generate_EnumValueWithoutCase_FactoryHasNoParametersAndNoData()
    {
        var source = Generate(Packet(@"<case value=""Data""><field name=""a"" type=""char""/></case>"));

        Assert.That(source, Does.Contain(
            "    public static TestReplyServerPacket ForMissing()\n" +
            "    {\n" +
            "        return new TestReplyServerPacket\n" +
            "        {\n" +
            "            ReplyCode = Code.Missing,\n" +
            "        };\n" +
            "    }\n"));
    }

    [Test]
    public void Generate_CaseWithoutSettableMembers_FactoryCreatesData()
    {
        var source = Generate(Packet(@"
<case value=""Hardcoded"">
    <field type=""string"">NO</field>
    <field name=""named"" type=""char"">1</field>
    <dummy type=""byte"">0</dummy>
</case>"));

        Assert.That(source, Does.Contain("public static TestReplyServerPacket ForHardcoded()\n"));
        Assert.That(source, Does.Contain("            ReplyCodeData = new ReplyCodeDataHardcoded(),\n"));
        Assert.That(source, Does.Contain("set to a new <see cref=\"ReplyCodeDataHardcoded\"/>."));
    }

    [Test]
    public void Generate_CaseWithLengthOfArray_FactoryTakesData()
    {
        var source = Generate(Packet(@"
<case value=""Data"">
    <length name=""items_count"" type=""char""/>
    <array name=""items"" type=""char"" length=""items_count""/>
</case>"));

        Assert.That(source, Does.Contain("public static TestReplyServerPacket ForData(ReplyCodeDataData data)\n"));
    }

    [Test]
    public void Generate_CaseWithUnreferencedLength_FactoryTakesData()
    {
        var source = Generate(Packet(@"<case value=""Data""><length name=""amount"" type=""char""/></case>"));

        Assert.That(source, Does.Contain("public static TestReplyServerPacket ForData(ReplyCodeDataData data)\n"));
    }

    [Test]
    public void Generate_NestedSwitch_FlattenedOntoOuterType()
    {
        var source = Generate(Packet(NestedCase(@"
<case value=""First""><field name=""a"" type=""char""/></case>
<case value=""Second""/>
<case value=""Third""><field type=""string"">NO</field></case>")));

        Assert.That(source, Does.Contain(
            "    public static TestReplyServerPacket ForNestedFirst(ReplyCodeDataNested.InnerTypeDataFirst data)\n" +
            "    {\n" +
            "        if (data == null)\n" +
            "        {\n" +
            "            throw new ArgumentNullException(nameof(data));\n" +
            "        }\n" +
            "\n" +
            "        return new TestReplyServerPacket\n" +
            "        {\n" +
            "            ReplyCode = Code.Nested,\n" +
            "            ReplyCodeData = new ReplyCodeDataNested\n" +
            "            {\n" +
            "                InnerType = Inner.First,\n" +
            "                InnerTypeData = data,\n" +
            "            },\n" +
            "        };\n" +
            "    }\n"));
        Assert.That(source, Does.Contain(
            "            ReplyCodeData = new ReplyCodeDataNested\n" +
            "            {\n" +
            "                InnerType = Inner.Second,\n" +
            "            },\n"));
        Assert.That(source, Does.Contain(
            "                InnerType = Inner.Third,\n" +
            "                InnerTypeData = new ReplyCodeDataNested.InnerTypeDataThird(),\n"));
        Assert.That(source, Does.Contain("public static TestReplyServerPacket ForNestedSecond()\n"));
        Assert.That(source, Does.Contain("public static TestReplyServerPacket ForNestedThird()\n"));
        Assert.That(source, Does.Contain(
            "with <see cref=\"ReplyCode\"/> set to <see cref=\"Code.Nested\"/>, <see cref=\"ReplyCodeDataNested.InnerType\"/> set to <see cref=\"Inner.First\"/> " +
            "and <see cref=\"ReplyCodeDataNested.InnerTypeData\"/> set to <paramref name=\"data\"/>."));
    }

    [Test]
    public void Generate_NestedSwitch_NoFactoryForOuterValueOrOnCaseType()
    {
        var source = Generate(Packet(NestedCase(@"<case value=""First""><field name=""a"" type=""char""/></case>")));

        Assert.That(source, Does.Not.Contain(" ForNested("));
        Assert.That(source, Does.Not.Contain("public static ReplyCodeDataNested "));
        Assert.That(source, Does.Not.Contain(" ForFirst("));
    }

    [Test]
    public void Generate_NumericCaseWithData_NamedAfterDataType()
    {
        var source = Generate(Packet(@"<case value=""0""><field name=""a"" type=""char""/></case>"));

        Assert.That(source, Does.Contain(
            "    public static TestReplyServerPacket ForReplyCodeData0(ReplyCodeData0 data)\n"));
        Assert.That(source, Does.Contain("            ReplyCode = (Code)0,\n"));
    }

    [Test]
    public void Generate_NestedNumericCaseWithData_FlattenedAndNamedAfterDataType()
    {
        var source = Generate(Packet(NestedCase(@"<case value=""0""><field name=""a"" type=""char""/></case>")));

        Assert.That(source, Does.Contain(
            "    public static TestReplyServerPacket ForInnerTypeData0(ReplyCodeDataNested.InnerTypeData0 data)\n"));
        Assert.That(source, Does.Contain(
            "            ReplyCode = Code.Nested,\n" +
            "            ReplyCodeData = new ReplyCodeDataNested\n" +
            "            {\n" +
            "                InnerType = (Inner)0,\n" +
            "                InnerTypeData = data,\n"));
    }

    [Test]
    public void Generate_NumericCaseWithoutData_Skipped()
    {
        var source = Generate(Packet(@"<case value=""0""/>"));

        Assert.That(source, Does.Not.Contain("ForReplyCodeData0"));
        Assert.That(source, Does.Not.Contain("(Code)0"));
    }

    [Test]
    public void Generate_DefaultCase_TakesCodeAndDataAndRejectsOtherCases()
    {
        var source = Generate(Packet(@"
<case value=""0""/>
<case value=""Data""><field name=""a"" type=""char""/></case>
<case value=""Empty""/>
<case value=""Missing""/>
<case value=""Hardcoded""/>
<case value=""Nested""/>
<case default=""true""><field name=""b"" type=""char""/></case>"));

        Assert.That(source, Does.Contain(
            "    /// <param name=\"code\">The value of <see cref=\"ReplyCode\"/>. Must not be a value that has its own case.</param>\n" +
            "    /// <param name=\"data\">The data for the case.</param>\n" +
            "    /// <returns>The new <see cref=\"TestReplyServerPacket\"/>.</returns>\n" +
            "    /// <exception cref=\"ArgumentException\">Thrown when <paramref name=\"code\"/> is a value that has its own case.</exception>\n" +
            "    /// <exception cref=\"ArgumentNullException\">Thrown when <paramref name=\"data\"/> is null.</exception>\n" +
            "    public static TestReplyServerPacket ForReplyCodeDefault(Code code, ReplyCodeDataDefault data)\n" +
            "    {\n" +
            "        if (code == (Code)0\n" +
            "            || code == Code.Data\n" +
            "            || code == Code.Empty\n" +
            "            || code == Code.Missing\n" +
            "            || code == Code.Hardcoded\n" +
            "            || code == Code.Nested)\n" +
            "        {\n" +
            "            throw new ArgumentException($\"Expected code to be a value without its own case, but was {code}\", nameof(code));\n" +
            "        }\n" +
            "\n" +
            "        if (data == null)\n" +
            "        {\n" +
            "            throw new ArgumentNullException(nameof(data));\n" +
            "        }\n" +
            "\n" +
            "        return new TestReplyServerPacket\n" +
            "        {\n" +
            "            ReplyCode = code,\n" +
            "            ReplyCodeData = data,\n" +
            "        };\n" +
            "    }\n"));
    }

    [Test]
    public void Generate_DefaultCase_EnumValuesWithoutCaseHaveNoFactory()
    {
        var source = Generate(Packet(@"
<case value=""Data""><field name=""a"" type=""char""/></case>
<case default=""true""><field type=""string"">OK</field></case>"));

        Assert.That(source, Does.Contain("public static TestReplyServerPacket ForData(ReplyCodeDataData data)\n"));
        Assert.That(source, Does.Contain("public static TestReplyServerPacket ForReplyCodeDefault(Code code)\n"));
        Assert.That(source, Does.Contain("            ReplyCodeData = new ReplyCodeDataDefault(),\n"));
        Assert.That(source, Does.Not.Contain(" ForEmpty("));
        Assert.That(source, Does.Not.Contain(" ForMissing("));
    }

    [Test]
    public void Generate_SwitchInStruct_FactoryOnStruct()
    {
        var source = Generate(@"
<struct name=""Entry"">
    <field name=""entry_type"" type=""Code""/>
    <switch field=""entry_type"">
        <case value=""Data""><field name=""a"" type=""short""/></case>
    </switch>
    <field name=""line"" type=""string""/>
</struct>");

        Assert.That(source, Does.Contain(
            "    public static Entry ForData(EntryTypeDataData data)\n"));
        Assert.That(source, Does.Contain(
            "        return new Entry\n" +
            "        {\n" +
            "            EntryType = Code.Data,\n" +
            "            EntryTypeData = data,\n" +
            "        };\n"));
    }

    [Test]
    public void Generate_SwitchInChunked_FactoryGenerated()
    {
        var source = Generate(@"
<packet family=""Test"" action=""Reply"">
    <chunked>
        <field name=""reply_code"" type=""Code""/>
        <switch field=""reply_code"">
            <case value=""Data""><field name=""a"" type=""char""/></case>
        </switch>
    </chunked>
</packet>");

        Assert.That(source, Does.Contain("public static TestReplyServerPacket ForData(ReplyCodeDataData data)\n"));
    }

    private static IEnumerable<TestCaseData> MultipleSwitches()
    {
        yield return new TestCaseData(@"
<packet family=""Test"" action=""Reply"">
    <field name=""reply_code"" type=""Code""/>
    <switch field=""reply_code""><case value=""Data""/></switch>
    <field name=""inner_type"" type=""Inner""/>
    <switch field=""inner_type""><case value=""First""/></switch>
</packet>",
            "reply_code and inner_type")
            .SetName("{m}(TypeScope)");
        yield return new TestCaseData(@"
<packet family=""Test"" action=""Reply"">
    <chunked>
        <field name=""reply_code"" type=""Code""/>
        <switch field=""reply_code""><case value=""Data""/></switch>
        <break/>
        <field name=""inner_type"" type=""Inner""/>
        <switch field=""inner_type""><case value=""First""/></switch>
    </chunked>
</packet>",
            "reply_code and inner_type")
            .SetName("{m}(AcrossChunks)");
        yield return new TestCaseData(Packet(@"
<case value=""Nested"">
    <field name=""inner_type"" type=""Inner""/>
    <switch field=""inner_type""><case value=""First""/></switch>
    <field name=""other_type"" type=""Inner""/>
    <switch field=""other_type""><case value=""First""/></switch>
</case>"),
            "inner_type and other_type")
            .SetName("{m}(CaseScope)");
    }

    [TestCaseSource(nameof(MultipleSwitches))]
    public void Validate_MultipleSwitchesInOneScope_ReportsError(string body, string switches)
    {
        var error = GenerateError(body);

        Assert.That(error, Does.Contain($"Test_Reply: Switch factories don't support multiple switches in one scope (switches on {switches})."));
    }

    private static string Packet(string cases) => $@"
<packet family=""Test"" action=""Reply"">
    <field name=""reply_code"" type=""Code""/>
    <switch field=""reply_code"">
        {cases}
    </switch>
</packet>";

    private static string NestedCase(string innerCases) => $@"
<case value=""Nested"">
    <field name=""inner_type"" type=""Inner""/>
    <switch field=""inner_type"">
        {innerCases}
    </switch>
</case>";

    private static string Generate(string body)
    {
        var result = Run(new ProtocolIncrementalGenerator(), ("xml/net/server/protocol.xml", ProtocolXml(Enums + body)));
        Assert.That(result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()), Is.Empty);
        return result.GeneratedTrees.Last().ToString().Replace("\r\n", "\n");
    }

    private static string GenerateError(string body)
    {
        var result = Run(new ProtocolIncrementalGenerator(), ("xml/net/server/protocol.xml", ProtocolXml(Enums + body)));
        var error = result.Diagnostics.Single(x => x.Id == "EO0004");
        Assert.That(error.Severity, Is.EqualTo(DiagnosticSeverity.Error));
        return error.GetMessage();
    }
}
