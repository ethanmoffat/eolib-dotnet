using static ProtocolGenerator.Test.GeneratorTestHarness;

namespace ProtocolGenerator.Test;

public class ProtocolIncrementalGeneratorTest
{
    private const string Xml = @"
<protocol>
    <enum name=""Direction"" type=""char"">
        <value name=""Down"">0</value>
        <value name=""Up"">1</value>
    </enum>
    <struct name=""Coords"">
        <field name=""x"" type=""char""/>
        <field name=""y"" type=""char""/>
    </struct>
</protocol>";

    [Test]
    public void Run_RepeatedInSameProcess_ReportsNoDuplicateTypes()
    {
        // The compiler server and IDE hosts keep the generator loaded, so every run after the first used to see the
        // types registered by earlier runs.
        var generator = new ProtocolIncrementalGenerator();

        var first = Run(generator, ("xml/protocol.xml", Xml));
        var second = Run(new ProtocolIncrementalGenerator(), ("xml/protocol.xml", Xml));
        var third = Run(generator, ("xml/protocol.xml", Xml));

        foreach (var result in new[] { first, second, third })
        {
            Assert.That(result.Diagnostics.Select(x => x.Id), Has.None.EqualTo("EO0003"));
            Assert.That(result.GeneratedTrees, Has.Length.EqualTo(PostInitializationSourceCount + 1));
        }
    }

    [Test]
    public void Run_DuplicateTypeInOneRun_ReportsDuplicateType()
    {
        var result = Run(new ProtocolIncrementalGenerator(), ("xml/protocol.xml", Xml), ("xml/other/protocol.xml", Xml));

        Assert.That(result.Diagnostics.Where(x => x.Id == "EO0003").Select(x => x.GetMessage()),
            Is.EquivalentTo(new[] { "Duplicate protocol type detected: Direction", "Duplicate protocol type detected: Coords" }));
    }
}
