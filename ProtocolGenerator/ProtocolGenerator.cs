using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Text;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator;

public class ProtocolGenerator
{
    private const string ProtocolNamespaceRoot = "EOLib.Protocol";

    private readonly ProtocolGeneratorOptions _options;
    private readonly string _filePath;
    private readonly SourceText _sourceText;

    public string HintName
    {
        get
        {
            var split = _filePath.Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            var parts = split
                .SkipWhile(x => x != "xml")
                .Skip(1)
                .TakeWhile(x => !x.Equals("protocol.xml", System.StringComparison.OrdinalIgnoreCase))
                .Select(x => char.ToUpper(x[0]) + x.Substring(1));
            var joinedParts = string.Join(".", parts);
            return string.IsNullOrWhiteSpace(joinedParts) ? "protocol.g.cs" : $"protocol.{joinedParts}.g.cs";
        }
    }

    private string Namespace
    {
        get
        {
            var ns = HintName;
            ns = ns.Replace("protocol", ProtocolNamespaceRoot);
            ns = ns.Replace(".g.cs", string.Empty);
            return ns;
        }
    }

    public ProtocolGenerator(ProtocolGeneratorOptions options, string filePath, SourceText sourceText)
    {
        _options = options;
        _filePath = filePath;
        _sourceText = sourceText;
    }

    public SourceText Generate()
    {
        var sourceTextString = _sourceText.ToString();
        using var ms = new MemoryStream(_sourceText.Encoding.GetBytes(sourceTextString));

        var serializer = new XmlSerializer(typeof(ProtocolSpec));
        var model = (ProtocolSpec)serializer.Deserialize(ms);

        var sb = new StringBuilder($"namespace {Namespace};");
        var typeRegistry = new Dictionary<string, string>();

        foreach (var type in model.Enums)
        {
            var generated = Generate(type, typeRegistry);
            sb.Append($"\n{generated}\n");
        }

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private string Generate(ProtocolEnum inputType, Dictionary<string, string> typeRegistry)
    {
        var state = new GeneratorState();

        state.Comment(inputType.Comment);
        state.Attribute("Generated");

        state.TypeDeclaration(GeneratorState.Visibility.Public, GeneratorState.ObjectType.Enum, inputType.Name, inputType.Type);
        state.BeginBlock();
        state.ValuesList(inputType.Values);
        state.EndBlock();

        return state.Output();
    }
}
