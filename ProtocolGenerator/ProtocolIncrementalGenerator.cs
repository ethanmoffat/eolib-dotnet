using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ProtocolGenerator.Model.Xml;
using ProtocolGenerator.Types;

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

        var parsedFiles = new List<(string Path, ProtocolSpec Spec)>();
        foreach (var file in filesFiltered)
        {
            var sourceTextString = file.Text.ToString();
            using var ms = new MemoryStream(file.Text.Encoding.GetBytes(sourceTextString));

            var serializer = new XmlSerializer(typeof(ProtocolSpec));
            var model = (ProtocolSpec)serializer.Deserialize(ms);

            foreach (var e in model.Enums)
                TypeMapper.Instance.RegisterEnum(e.Name, e.Type);

            foreach (var s in model.Structs)
                TypeMapper.Instance.RegisterStruct(s.Name, s);

            parsedFiles.Add((file.Path, model));
        }

        foreach (var file in parsedFiles)
        {
            context.ReportDiagnostic(Diagnostic.Create(ddForFile, Location.None, file.Path));

            var generator = new ProtocolGenerator(options, file.Path, file.Spec);
            context.AddSource(
                generator.HintName,
                generator.Generate());
        }
    }
}
