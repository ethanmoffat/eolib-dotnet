using static ProtocolGenerator.Test.GeneratorTestHarness;

namespace ProtocolGenerator.Test;

public class ProtocolValidatorTest
{
    private const string Enum = @"<enum name=""Kind"" type=""char""><value name=""A"">1</value></enum>";

    private static IEnumerable<TestCaseData> ValidStructs()
    {
        yield return new TestCaseData(@"<field name=""a"" type=""char""/><array name=""b"" type=""char""/>").SetName("{m}(UnsizedArrayLast)");
        yield return new TestCaseData(@"<chunked><array name=""a"" type=""char""/><break/><field name=""b"" type=""char""/></chunked>").SetName("{m}(UnsizedArrayBeforeBreak)");
        yield return new TestCaseData(@"<field name=""a"" type=""char""/><field name=""b"" type=""char"" optional=""true""/><field name=""c"" type=""char"" optional=""true""/>").SetName("{m}(OptionalFieldsLast)");
        yield return new TestCaseData(@"<chunked><field name=""a"" type=""char"" optional=""true""/><break/><field name=""b"" type=""char""/></chunked>").SetName("{m}(OptionalResetByBreak)");
        yield return new TestCaseData(@"<chunked><array name=""a"" type=""string"" delimited=""true""/></chunked>").SetName("{m}(DelimitedInChunked)");
        yield return new TestCaseData(@"<field name=""a"" type=""char""/><dummy type=""byte"">0</dummy>").SetName("{m}(DummyLast)");
        yield return new TestCaseData(@"<field name=""a"" type=""string"" length=""3"" padded=""true""/>").SetName("{m}(PaddedStringWithLength)");
        yield return new TestCaseData(@"<length name=""a_len"" type=""char""/><field name=""a"" type=""string"" length=""a_len""/>").SetName("{m}(LengthReferencedOnce)");
        yield return new TestCaseData(@"<field name=""a"" type=""Kind:short""/><field name=""b"" type=""bool:short""/>").SetName("{m}(UnderlyingTypes)");
        yield return new TestCaseData(@"<field name=""a"" type=""Kind""/><switch field=""a""><case value=""A""><field name=""b"" type=""char""/></case><case default=""true""/></switch>").SetName("{m}(SwitchWithDefaultLast)");
    }

    private static IEnumerable<TestCaseData> InvalidStructs()
    {
        yield return new TestCaseData(@"<array name=""a"" type=""char""/><field name=""b"" type=""char""/>", "Non-delimited arrays without a length must be the final element").SetName("{m}(UnsizedArrayNotLast)");
        yield return new TestCaseData(@"<field name=""a"" type=""char"" optional=""true""/><field name=""b"" type=""char""/>", "Optional fields may not be followed by non-optional fields (b)").SetName("{m}(RequiredAfterOptional)");
        yield return new TestCaseData(@"<array name=""a"" type=""string"" delimited=""true""/>", "Delimited arrays are only allowed in chunked sections (a)").SetName("{m}(DelimitedOutsideChunked)");
        yield return new TestCaseData(@"<dummy type=""byte"">0</dummy><field name=""a"" type=""char""/>", "<dummy> elements must not be followed by any other elements").SetName("{m}(DummyNotLast)");
        yield return new TestCaseData(@"<field name=""a"" type=""string"" padded=""true""/>", "Padded fields must specify a length (a)").SetName("{m}(PaddedWithoutLength)");
        yield return new TestCaseData(@"<field name=""a"" type=""char"" length=""3""/>", "The length attribute is only allowed for string types (a)").SetName("{m}(LengthOnNonString)");
        yield return new TestCaseData(@"<field name=""a"" type=""char"" padded=""true""/>", "The padded attribute is only allowed for string types (a)").SetName("{m}(PaddedOnNonString)");
        yield return new TestCaseData(@"<length name=""len"" type=""char""/><field name=""a"" type=""string"" length=""len""/><field name=""b"" type=""string"" length=""len""/>", "Length field len must not be referenced by multiple fields (a, b)").SetName("{m}(LengthReferencedTwice)");
        yield return new TestCaseData(@"<field name=""a"" type=""Kind:string""/>", "Underlying type string must be an integer type (a)").SetName("{m}(NonIntegerUnderlyingType)");
        yield return new TestCaseData(@"<field name=""a"" type=""string:char""/>", "Only integer, bool and enum types can specify an underlying type (a: string:char)").SetName("{m}(UnderlyingTypeOnString)");
        yield return new TestCaseData(@"<switch field=""a""><case value=""A""/></switch>", "Switch must reference a preceding field (a)").SetName("{m}(SwitchUnknownField)");
        yield return new TestCaseData(@"<field name=""a"" type=""Kind""/><switch field=""a""><case default=""true""/><case value=""A""/></switch>", "Only the last case in a switch on a can be the default case").SetName("{m}(SwitchDefaultNotLast)");
        yield return new TestCaseData(@"<field name=""a"" type=""Kind""/><switch field=""a""><case value=""A""/><case value=""A""/></switch>", "Duplicate case value A in switch on a").SetName("{m}(SwitchDuplicateCase)");
    }

    [TestCaseSource(nameof(ValidStructs))]
    public void Validate_ValidStruct_ReportsNoError(string structBody)
    {
        var result = Run(new ProtocolIncrementalGenerator(), ("xml/protocol.xml", ProtocolXml(Enum + $@"<struct name=""S"">{structBody}</struct>")));

        Assert.That(result.Diagnostics.Where(x => x.Id == "EO0004").Select(x => x.GetMessage()), Is.Empty);
        Assert.That(result.GeneratedTrees, Has.Length.EqualTo(PostInitializationSourceCount + 1));
    }

    [TestCaseSource(nameof(InvalidStructs))]
    public void Validate_InvalidStruct_ReportsError(string structBody, string expectedMessage)
    {
        var result = Run(new ProtocolIncrementalGenerator(), ("xml/protocol.xml", ProtocolXml(Enum + $@"<struct name=""S"">{structBody}</struct>")));

        var error = result.Diagnostics.Single(x => x.Id == "EO0004");
        Assert.That(error.Severity, Is.EqualTo(Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
        Assert.That(error.GetMessage(), Does.Contain($"S: {expectedMessage}"));
        Assert.That(result.GeneratedTrees, Has.Length.EqualTo(PostInitializationSourceCount), "Only the post-initialization output should be generated");
    }
}
