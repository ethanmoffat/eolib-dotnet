using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ProtocolGenerator;

[Generator]
public class ProtocolIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitOutput.CreatePacketInterface);

        var generatorOptions = context.AnalyzerConfigOptionsProvider.Select(static (configOptions, _) =>
        {
            if (!configOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var projectRoot) ||
                !configOptions.GlobalOptions.TryGetValue(ProtocolGeneratorOptions.InputDirectoryOption, out var inputDirectory))
                return ProtocolGeneratorOptions.Empty;

            return new ProtocolGeneratorOptions(projectRoot, inputDirectory);
        });

        var xmlFiles = context.AdditionalTextsProvider.Where(static f => f.Path.EndsWith(".xml")).Select((f, _) => (f.Path, Text: f.GetText()));
        var multiValues = generatorOptions.Combine(xmlFiles.Collect());
        context.RegisterSourceOutput(multiValues, GenerateProtocol);
    }

    private void Test(SourceProductionContext context, ImmutableArray<(SyntaxNode Node, SemanticModel SemanticModel)> array)
    {
        foreach (var item in array)
        {
            var ddForFile = new DiagnosticDescriptor("EO0003", "EO Protocol Test", "{0}", "EO.Generation", DiagnosticSeverity.Warning, true);
            context.ReportDiagnostic(Diagnostic.Create(ddForFile, Location.None, item.Node.SyntaxTree.FilePath));
        }
    }

    private void GenerateProtocol(SourceProductionContext context, (ProtocolGeneratorOptions Options, ImmutableArray<(string Path, SourceText Text)> Files) inputs)
    {
        var options = inputs.Options;

        var filesFiltered = inputs.Files.Where(x => x.Path.StartsWith(Path.Combine(options.ProjectRoot, options.InputDirectory)));

        var ddForGeneration = new DiagnosticDescriptor("EO0001", "EO Protocol Generation", "Generating EO protocol from {0}", "EO.Generation", DiagnosticSeverity.Warning, true);
        context.ReportDiagnostic(Diagnostic.Create(ddForGeneration, Location.None, options.InputDirectory));

        var ddForFile = new DiagnosticDescriptor("EO0002", "EO Protocol File Info", "Generating protocol for: {0}", "EO.Generation", DiagnosticSeverity.Warning, true);
        foreach (var file in filesFiltered)
        {
            context.ReportDiagnostic(Diagnostic.Create(ddForFile, Location.None, file.Path));

            var generator = new ProtocolGenerator(options, file.Path, file.Text);
            context.AddSource(
                generator.HintName,
                generator.Generate());
        }
    }
}
