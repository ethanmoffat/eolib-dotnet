using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ProtocolGenerator;

[Generator]
public class EoProtocolGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var generatorOptions = context.AnalyzerConfigOptionsProvider.Select(static (configOptions, _) =>
        {
            if (!configOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var projectRoot) ||
                !configOptions.GlobalOptions.TryGetValue(ProtocolGeneratorOptions.InputDirectoryOption, out var inputDirectory))
                return ProtocolGeneratorOptions.Empty;

            return new ProtocolGeneratorOptions(projectRoot, inputDirectory);
        });

        var xmlFiles = context.AdditionalTextsProvider.Where(f => f.Path.EndsWith(".xml")).Select((f, _) => (f.Path, Text: f.GetText()));
        var multiValues = generatorOptions.Combine(xmlFiles.Collect());
        context.RegisterSourceOutput(multiValues, GenerateProtocol);
    }

    private void GenerateProtocol(SourceProductionContext context, (ProtocolGeneratorOptions Options, ImmutableArray<(string Path, SourceText Text)> Files) inputs)
    {
        var options = inputs.Options;

        var filesFiltered = inputs.Files.Where(x => x.Path.StartsWith(Path.Combine(options.ProjectRoot, options.InputDirectory)));

        var descriptor = new DiagnosticDescriptor("EO0001", "EO Protocol Generation Warning", "Generating EO protocol from {0}", "EO.Generation", DiagnosticSeverity.Warning, true);
        context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, options.InputDirectory));

        var descriptor2 = new DiagnosticDescriptor("EO0002", "EO Protocol Files List", "file pair: {0}, {1} lines", "EO.Generation", DiagnosticSeverity.Warning, true);
        foreach (var file in filesFiltered)
        {
            context.ReportDiagnostic(Diagnostic.Create(descriptor2, Location.None, file.Path, file.Text.Lines.Count));
        }

        // context.AddSource(
        //     Path.Combine(options.ProjectRoot, "options.g.cs"),
        //     SourceText.From($"{options.InputDirectory}\n{options.OutputDirectory}\n", Encoding.UTF8));
    }
}
