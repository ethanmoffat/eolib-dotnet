using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ProtocolGenerator.Extensions;
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

        var ddForGeneration = new DiagnosticDescriptor("EO0001", "EO Protocol Generation", "Generating EO protocol from {0}", "EO.Generation", DiagnosticSeverity.Info, true);
        var ddForFile = new DiagnosticDescriptor("EO0002", "EO Protocol File Info", "Generating protocol for: {0}", "EO.Generation", DiagnosticSeverity.Info, true);
        var ddDuplicateTypeWarning = new DiagnosticDescriptor("EO0003", "EO Protocol duplicate type", "Duplicate protocol type detected: {0}", "EO.Generation", DiagnosticSeverity.Warning, true);
        var ddInvalidProtocolError = new DiagnosticDescriptor("EO0004", "EO Protocol invalid", "Invalid protocol in {0}: {1}", "EO.Generation", DiagnosticSeverity.Error, true);

        context.ReportDiagnostic(Diagnostic.Create(ddForGeneration, Location.None, options.InputDirectory));

        // A new TypeMapper per run: the compiler server and IDE reuse the generator instance across compilations
        var typeMapper = new TypeMapper();
        var parsedFiles = new List<(string Path, ProtocolSpec Spec)>();
        foreach (var file in filesFiltered)
        {
            var document = XDocument.Parse(file.Text.ToString(), LoadOptions.PreserveWhitespace);
            document.Root.RewriteCommentsAsElementsInPlace();

            var serializer = new XmlSerializer(typeof(ProtocolSpec));
            var model = (ProtocolSpec)serializer.Deserialize(document.CreateReader());

            foreach (var e in model.Enums)
            {
                if (!typeMapper.RegisterEnum(e.Name, e.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ddDuplicateTypeWarning, Location.None, e.Name));
                }
            }

            foreach (var s in model.Structs)
            {
                if (!typeMapper.RegisterStruct(s.Name, s))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ddDuplicateTypeWarning, Location.None, s.Name));
                }
            }

            parsedFiles.Add((file.Path, model));
        }

        var validator = new ProtocolValidator(typeMapper);
        foreach (var file in parsedFiles)
        {
            try
            {
                validator.Validate(file.Spec);
            }
            catch (InvalidOperationException ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(ddInvalidProtocolError, Location.None, file.Path, ex.Message));
                return;
            }
        }

        foreach (var file in parsedFiles)
        {
            context.ReportDiagnostic(Diagnostic.Create(ddForFile, Location.None, file.Path));

            var generator = new ProtocolGenerator(options, file.Path, file.Spec, typeMapper);
            context.AddSource(
                generator.HintName,
                generator.Generate());
        }
    }
}
