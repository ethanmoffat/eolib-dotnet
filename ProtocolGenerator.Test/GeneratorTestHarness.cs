using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ProtocolGenerator.Test;

/// <summary>
/// Runs <see cref="ProtocolIncrementalGenerator"/> through a Roslyn generator driver against in-memory protocol XML.
/// </summary>
internal static class GeneratorTestHarness
{
    private static readonly string ProjectRoot = Path.Combine(Path.GetTempPath(), "eolib-generator-test");
    private const string InputDirectory = "eo-protocol";

    /// <summary>
    /// Gets the number of sources generated regardless of protocol input (IPacket and global usings).
    /// </summary>
    public const int PostInitializationSourceCount = 2;

    public static GeneratorDriverRunResult Run(ProtocolIncrementalGenerator generator, params (string RelativePath, string Xml)[] files)
    {
        var additionalTexts = files
            .Select(f => (AdditionalText)new InMemoryAdditionalText(Path.Combine(ProjectRoot, InputDirectory, f.RelativePath.Replace('/', Path.DirectorySeparatorChar)), f.Xml))
            .ToImmutableArray();

        var driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            additionalTexts,
            optionsProvider: new TestOptionsProvider(new Dictionary<string, string>
            {
                ["build_property.projectdir"] = ProjectRoot,
                [ProtocolGeneratorOptions.InputDirectoryOption] = InputDirectory,
            }));

        var result = driver.RunGenerators(CSharpCompilation.Create("GeneratorTest")).GetRunResult();
        Assert.That(result.Results.Select(x => x.Exception), Has.All.Null, "Generator threw an exception");
        return result;
    }

    public static string ProtocolXml(string body) => $"<protocol>{body}</protocol>";

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _text;

        public InMemoryAdditionalText(string path, string text)
        {
            Path = path;
            _text = SourceText.From(text, Encoding.UTF8);
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }

    private sealed class TestOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions EmptyOptions = new TestOptions(new Dictionary<string, string>());

        public TestOptionsProvider(Dictionary<string, string> globalOptions)
        {
            GlobalOptions = new TestOptions(globalOptions);
        }

        public override AnalyzerConfigOptions GlobalOptions { get; }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => EmptyOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => EmptyOptions;
    }

    private sealed class TestOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _options;

        public TestOptions(Dictionary<string, string> options)
        {
            _options = options;
        }

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _options.TryGetValue(key, out value);
    }
}
