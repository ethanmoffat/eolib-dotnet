using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Text;
using ProtocolGenerator.Model.Protocol;
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
            var split = _filePath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var parts = split
                .SkipWhile(x => !x.Equals("xml", StringComparison.OrdinalIgnoreCase))
                .Skip(1)
                .TakeWhile(x => !x.Equals("protocol.xml", StringComparison.OrdinalIgnoreCase))
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
        var allSpecs = model.Enums.Concat<object>(model.Packets).Concat(model.Structs);

        var sb = new StringBuilder($"namespace {Namespace};");
        foreach (var type in allSpecs)
        {
            var state = new GeneratorState();

            if (type is ProtocolEnum e)
                Generate(e, state);
            else if (type is ProtocolStruct s)
                Generate(s, state);
            else if (type is ProtocolPacket p)
                Generate(p, state);
            else
                continue;

            sb.AppendLine(state.Output());
        }

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private void Generate(ProtocolEnum inputType, GeneratorState state)
    {
        state.Comment(inputType.Comment);
        state.Attribute("Generated");

        state.TypeDeclaration(GeneratorState.Visibility.Public, GeneratorState.ObjectType.Enum, inputType.Name, inputType.Type);
        state.BeginBlock();
        state.ValuesList(inputType.Values);
        state.EndBlock();
    }

    private void Generate(ProtocolStruct inputType, GeneratorState state)
    {
        state.Comment(inputType.Comment);
        state.Attribute("Generated");

        state.TypeDeclaration(
            GeneratorState.Visibility.Public,
            inputType.IsInterface ? GeneratorState.ObjectType.Interface : GeneratorState.ObjectType.Class,
            inputType.Name,
            string.IsNullOrWhiteSpace(inputType.BaseType) ? string.Empty : inputType.BaseType
        );
        state.BeginBlock();
        if (!inputType.IsInterface)
        {
            GenerateStructureImplementation(state, inputType.Name, inputType.Instructions.Select(ProtocolInstructionFactory.Transform).ToList());
        }
        state.EndBlock();
    }

    private void Generate(ProtocolPacket inputType, GeneratorState state)
    {
        state.Comment(inputType.Comment);
        state.Attribute("Generated");

        var clientOrServer = HintName.Contains("Client")
            ? "Client"
            : HintName.Contains("Server")
                ? "Server"
                : string.Empty;
        var typeName = $"{inputType.Family}{inputType.Action}{clientOrServer}Packet";

        state.TypeDeclaration(GeneratorState.Visibility.Public, GeneratorState.ObjectType.Class, typeName, "IPacket");
        state.BeginBlock();
        state.AutoProperty(
            GeneratorState.Visibility.Public,
            "PacketFamily",
            "Family",
            $"PacketFamily.{inputType.Family}"
        );
        state.NewLine();
        state.AutoProperty(
            GeneratorState.Visibility.Public,
            "PacketAction",
            "Action",
            $"PacketAction.{inputType.Action}"
        );
        state.NewLine();
        GenerateStructureImplementation(state, typeName, inputType.Instructions.Select(ProtocolInstructionFactory.Transform).ToList());
        state.EndBlock();
    }

    private void GenerateStructureImplementation(GeneratorState state, string typeName, List<IProtocolInstruction> instructions)
    {
        // Generate nested types. Each switch case is represented by a nested structure with data relevant to the switch case.
        // The switch case as a member is represented by an interface, with each "case" being a different implementation of that interface.
        foreach (var inst in instructions)
        {
            foreach (var nestedType in inst.GetNestedTypes())
            {
                Generate(nestedType, state);
                state.NewLine();
            }
        }

        // Generate properties. Most instructions are represented in a structure by a property.
        // A property may be a primitive type or a structure defined elsewhere in the protocol.
        foreach (var inst in instructions)
        {
            inst.GenerateProperty(state);

            if (inst.HasProperty)
                state.NewLine();
        }

        // Generate the Serialize method for the structure.
        state.MethodDeclaration(
            GeneratorState.Visibility.Public, "void", "Serialize", new List<(string, string)> { ("EoWriter", "writer") }
        );
        state.BeginBlock();
        foreach (var inst in instructions)
        {
            inst.GenerateSerialize(state);
        }
        state.EndBlock();
        state.NewLine();

        // Generate the Deserialize method for the structure.
        state.MethodDeclaration(
            GeneratorState.Visibility.Public, "void", "Deserialize", new List<(string, string)> { ("EoReader", "reader") }
        );
        state.BeginBlock();
        foreach (var inst in instructions)
        {
            inst.GenerateDeserialize(state);
        }
        state.EndBlock();
        state.NewLine();

        var flattenedInstructions = Flatten(instructions);

        // Generate ToString override
        state.MethodDeclaration(
            GeneratorState.Visibility.Public, "override string", "ToString", new List<(string, string)>()
        );
        state.BeginBlock();

        state.Return(endStatement: false);
        state.Text($"\"{typeName}{{\"", indented: false);
        state.IncreaseIndent();
        var memberIndex = 0;
        foreach (var inst in flattenedInstructions.Where(x => x.HasProperty))
        {
            state.NewLine();
            state.Text($"+ {(memberIndex != 0 ? "\",\" + " : string.Empty)}", indented: true);
            inst.GenerateToString(state);
            memberIndex++;
        }
        state.NewLine();
        state.Text("+ \"}\";", indented: true);
        state.NewLine();
        state.DecreaseIndent();

        state.EndBlock();
        state.NewLine();

        // Generate Equals override
        state.MethodDeclaration(
            GeneratorState.Visibility.Public, "override bool", "Equals", new List<(string, string)> { ("object", "other") }
        );
        state.BeginBlock();
        state.Return("false"); // todo: state.Return(endStatement: false);
        foreach (var inst in instructions)
        {
            inst.GenerateEquals(state);
        }
        state.EndBlock();
        state.NewLine();

        // Generate GetHashCode override
        state.MethodDeclaration(
            GeneratorState.Visibility.Public, "override int", "GetHashCode", new List<(string, string)>()
        );
        state.BeginBlock();
        state.Return("0"); // todo: state.Return(endStatement: false);
        foreach (var inst in instructions)
        {
            inst.GenerateGetHashCode(state);
        }
        state.EndBlock();
    }

    private static List<IProtocolInstruction> Flatten(List<IProtocolInstruction> instructions)
    {
        var retList = new List<IProtocolInstruction>();
        for (int i = 0; i < instructions.Count; i++)
        {
            if (instructions[i].Instructions.Count > 0)
                retList.AddRange(instructions[i].Instructions);
            else
                retList.Add(instructions[i]);
        }
        return retList;
    }
}
