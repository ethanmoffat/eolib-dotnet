using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using ProtocolGenerator.Extensions;
using ProtocolGenerator.Model.Protocol;
using ProtocolGenerator.Model.Xml;
using ProtocolGenerator.Types;

namespace ProtocolGenerator;

public class ProtocolGenerator
{
    private const string ProtocolNamespaceRoot = "Moffat.EndlessOnline.SDK.Protocol";

    private readonly ProtocolGeneratorOptions _options;
    private readonly string _filePath;
    private readonly ProtocolSpec _fullSpec;
    private readonly TypeMapper _typeMapper;

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

    public ProtocolGenerator(ProtocolGeneratorOptions options, string filePath, ProtocolSpec fullSpec, TypeMapper typeMapper)
    {
        _options = options;
        _filePath = filePath;
        _fullSpec = fullSpec;
        _typeMapper = typeMapper;
    }

    public SourceText Generate()
    {
        var allSpecs = _fullSpec.Enums.Concat<object>(_fullSpec.Structs).Concat(_fullSpec.Packets);

        var sb = new StringBuilder($"namespace {Namespace};\n\n#pragma warning disable 1591\n\n");
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

        sb.AppendLine("#pragma warning restore 1591\n");

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
        if (string.IsNullOrWhiteSpace(inputType.BaseType))
        {
            // Only apply "IsChunked" to nested elements when there is no base type.
            // Presence of a base type indicates this is a switch struct, meaning IsChunked has already been determined.
            // Applying chunked again will overwrite the correct value with 'false' (unless another chunked element is present).
            ApplyChunked(inputType.Instructions);
            AssociateLengths(inputType.Instructions);
        }

        var instructions = inputType.Instructions.Select(x => ProtocolInstructionFactory.Transform(_typeMapper, x)).ToList();

        state.Comment(inputType.Comment, GetInstructionNotes(instructions));
        state.Attribute("Generated");

        state.TypeDeclaration(
            GeneratorState.Visibility.Public,
            inputType.IsInterface ? GeneratorState.ObjectType.Interface : GeneratorState.ObjectType.Class,
            inputType.Name,
            string.IsNullOrWhiteSpace(inputType.BaseType) ? "ISerializable" : inputType.BaseType
        );
        state.BeginBlock();
        if (!inputType.IsInterface)
        {
            // Switch case types don't get factories: the factories of the containing type set their data
            var factories = string.IsNullOrWhiteSpace(inputType.BaseType)
                ? SwitchFactoryBuilder.Build(inputType.Instructions, _typeMapper)
                : new List<SwitchFactory>();
            GenerateStructureImplementation(state, inputType.Name, instructions, factories);
        }
        state.EndBlock();
    }

    private void Generate(ProtocolPacket inputType, GeneratorState state)
    {
        ApplyChunked(inputType.Instructions);
        AssociateLengths(inputType.Instructions);

        var instructions = inputType.Instructions.Select(x => ProtocolInstructionFactory.Transform(_typeMapper, x)).ToList();

        state.Comment(inputType.Comment, GetInstructionNotes(instructions));
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
        GenerateStructureImplementation(state, typeName, instructions, SwitchFactoryBuilder.Build(inputType.Instructions, _typeMapper));
        state.EndBlock();
    }

    private void GenerateStructureImplementation(GeneratorState state, string typeName, List<IProtocolInstruction> instructions, IReadOnlyList<SwitchFactory> factories)
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

        // Generate ByteSize property. This property is required for parity with how the client handles certain packets.
        // See CHEST_CLOSE packet and CharacterMapInfo struct.
        state.Comment("Gets the size of the data that this object was deserialized from, or 0 for an object that was not deserialized.");
        state.Property(GeneratorState.Visibility.Public, "int", "ByteSize", newLine: false);
        state.Text(" ", indented: false);
        state.BeginBlock(newLine: false, indented: false);
        state.Text(" ", indented: false);
        state.AutoGet(GeneratorState.Visibility.None, newLine: false, indented: false);
        state.Text(" ", indented: false);
        state.AutoSet(GeneratorState.Visibility.Private, newLine: false, indented: false);
        state.Text(" ", indented: false);
        state.EndBlock(indented: false);
        state.NewLine();

        // Generate properties. Most instructions are represented in a structure by a property.
        // A property may be a primitive type or a structure defined elsewhere in the protocol.
        foreach (var inst in instructions)
        {
            inst.GenerateProperty(state);

            if (inst.HasProperty)
            {
                state.NewLine();
                state.NewLine();
            }
        }

        foreach (var factory in factories)
        {
            GenerateSwitchFactory(state, typeName, factory);
            state.NewLine();
        }

        var flattenedInstructions = Flatten(instructions);
        flattenedInstructions.Insert(0, new FieldInstruction(new ProtocolFieldInstruction { Name = "ByteSize", Type = "int" }, _typeMapper));

        GenerateSerialize(state, instructions, flattenedInstructions);
        state.NewLine();

        GenerateDeserialize(state, instructions, flattenedInstructions);
        state.NewLine();

        GenerateToString(state, typeName, flattenedInstructions);
        state.NewLine();

        GenerateEquals(state, typeName, flattenedInstructions);
        state.NewLine();

        GenerateGetHashCode(state, typeName, flattenedInstructions);
    }

    private static void GenerateSwitchFactory(GeneratorState state, string typeName, SwitchFactory factory)
    {
        var leaf = factory.Leaf;

        GenerateSwitchFactoryComment(state, typeName, factory);

        var parameters = new List<(string, string)>();
        if (factory.IsDefault)
            parameters.Add((factory.DefaultCodeType, "code"));
        if (leaf.DataIsParameter)
            parameters.Add((leaf.DataTypeName, "data"));

        state.MethodDeclaration(GeneratorState.Visibility.Public, $"static {typeName}", factory.Name, parameters);
        state.BeginBlock();

        if (factory.IsDefault && factory.ExcludedCodes.Count > 0)
        {
            state.Text($"if (code == {factory.ExcludedCodes[0]}", indented: true);
            state.IncreaseIndent();
            foreach (var excluded in factory.ExcludedCodes.Skip(1))
            {
                state.NewLine();
                state.Text($"|| code == {excluded}", indented: true);
            }
            state.Text(")", indented: false);
            state.NewLine();
            state.DecreaseIndent();
            state.BeginBlock();
            state.Text($"throw new ArgumentException($\"Expected code to be a value without its own case, but was {{code}}\", nameof(code));", indented: true);
            state.NewLine();
            state.EndBlock();
            state.NewLine();
        }

        if (leaf.DataIsParameter)
        {
            state.Text("if (data == null)", indented: true);
            state.NewLine();
            state.BeginBlock();
            state.Text("throw new ArgumentNullException(nameof(data));", indented: true);
            state.NewLine();
            state.EndBlock();
            state.NewLine();
        }

        state.Return($"new {typeName}", endStatement: false);
        state.NewLine();
        for (var i = 0; i < factory.Steps.Count; i++)
        {
            var step = factory.Steps[i];
            var isLeaf = i == factory.Steps.Count - 1;

            state.BeginBlock();
            state.Text($"{step.FieldName} = {(factory.IsDefault ? "code" : step.CodeExpression)},", indented: true);
            state.NewLine();

            if (!isLeaf)
            {
                state.Text($"{step.DataPropertyName} = new {step.DataTypeName}", indented: true);
                state.NewLine();
                continue;
            }

            if (step.DataIsParameter)
            {
                state.Text($"{step.DataPropertyName} = data,", indented: true);
                state.NewLine();
            }
            else if (!string.IsNullOrEmpty(step.DataTypeName))
            {
                state.Text($"{step.DataPropertyName} = new {step.DataTypeName}(),", indented: true);
                state.NewLine();
            }
        }

        for (var i = factory.Steps.Count - 1; i >= 0; i--)
        {
            state.EndBlock(newLine: false);
            state.Text(i == 0 ? ";" : ",", indented: false);
            state.NewLine();
        }

        state.EndBlock();
    }

    private static void GenerateSwitchFactoryComment(GeneratorState state, string typeName, SwitchFactory factory)
    {
        var leaf = factory.Leaf;

        var assignments = factory.Steps
            .Select(x => $"<see cref=\"{x.QualifiedFieldName}\"/> set to {x.CodeDocumentation}")
            .ToList();

        if (leaf.DataIsParameter)
            assignments.Add($"<see cref=\"{leaf.QualifiedDataPropertyName}\"/> set to <paramref name=\"data\"/>");
        else if (!string.IsNullOrEmpty(leaf.DataTypeName))
            assignments.Add($"<see cref=\"{leaf.QualifiedDataPropertyName}\"/> set to a new <see cref=\"{leaf.DataTypeName}\"/>");

        var joinedAssignments = assignments.Count == 1
            ? assignments[0]
            : $"{string.Join(", ", assignments.Take(assignments.Count - 1))} and {assignments[assignments.Count - 1]}";

        state.Comment($"Creates a new <see cref=\"{typeName}\"/> with {joinedAssignments}.");

        if (factory.IsDefault)
            state.CommentTag("param", $"The value of <see cref=\"{leaf.QualifiedFieldName}\"/>. Must not be a value that has its own case.", "name=\"code\"");
        if (leaf.DataIsParameter)
            state.CommentTag("param", "The data for the case.", "name=\"data\"");

        state.CommentTag("returns", $"The new <see cref=\"{typeName}\"/>.");

        if (factory.IsDefault && factory.ExcludedCodes.Count > 0)
            state.CommentTag("exception", "Thrown when <paramref name=\"code\"/> is a value that has its own case.", "cref=\"ArgumentException\"");
        if (leaf.DataIsParameter)
            state.CommentTag("exception", "Thrown when <paramref name=\"data\"/> is null.", "cref=\"ArgumentNullException\"");
    }

    private static void GenerateSerialize(GeneratorState state, List<IProtocolInstruction> instructions, IReadOnlyList<IProtocolInstruction> flattenedInstructions)
    {
        state.MethodDeclaration(
            GeneratorState.Visibility.Public, "void", "Serialize", new List<(string, string)> { ("EoWriter", "writer") }
        );
        state.BeginBlock();

        var hasChunked = instructions.Any(x => x is ChunkedInstruction);
        var hasDummy = instructions.Any(x => x is DummyInstruction);

        if (hasChunked)
        {
            state.Text("var oldStringSanitization = writer.StringSanitization;", indented: true);
            state.NewLine();

            state.Text("try", indented: true);
            state.NewLine();
            state.BeginBlock();
        }

        if (hasDummy && flattenedInstructions.Count > 2)
        {
            state.Text("var oldWriterLength = writer.Length;", indented: true);
            state.NewLine();
        }

        foreach (var inst in instructions)
        {
            inst.GenerateSerialize(state, flattenedInstructions);
        }

        if (hasChunked)
        {
            state.EndBlock();
            state.Text("finally", indented: true);
            state.NewLine();
            state.BeginBlock();
            state.Text("writer.StringSanitization = oldStringSanitization;", indented: true);
            state.NewLine();
            state.EndBlock();
        }

        state.EndBlock();
    }

    private static void GenerateDeserialize(GeneratorState state, List<IProtocolInstruction> instructions, IReadOnlyList<IProtocolInstruction> flattenedInstructions)
    {
        state.MethodDeclaration(
            GeneratorState.Visibility.Public, "void", "Deserialize", new List<(string, string)> { ("EoReader", "reader") }
        );
        state.BeginBlock();

        var hasChunked = instructions.Any(x => x is ChunkedInstruction);

        if (hasChunked)
        {
            state.Text("var oldChunkedReadingMode = reader.ChunkedReadingMode;", indented: true);
            state.NewLine();

            state.Text("try", indented: true);
            state.NewLine();
            state.BeginBlock();
        }

        state.Text("var readerStartPosition = reader.Position;", indented: true);
        state.NewLine();

        foreach (var inst in instructions)
        {
            inst.GenerateDeserialize(state, flattenedInstructions);
        }

        state.Text("ByteSize = reader.Position - readerStartPosition;", indented: true);
        state.NewLine();

        if (hasChunked)
        {
            state.EndBlock();
            state.Text("finally", indented: true);
            state.NewLine();
            state.BeginBlock();
            state.Text("reader.ChunkedReadingMode = oldChunkedReadingMode;", indented: true);
            state.NewLine();
            state.EndBlock();
        }

        state.EndBlock();
    }

    private static void GenerateToString(GeneratorState state, string typeName, List<IProtocolInstruction> flattenedInstructions)
    {
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
    }

    private static void GenerateEquals(GeneratorState state, string typeName, List<IProtocolInstruction> instructions)
    {
        state.MethodDeclaration(
            GeneratorState.Visibility.Public, "override bool", "Equals", new List<(string, string)> { ("object", "other") }
        );
        state.BeginBlock();

        state.Text("if (this == other) return true;", indented: true);
        state.NewLine();
        state.NewLine();

        state.Text($"if (other is not {typeName} rhs) return false;", indented: true);
        state.NewLine();
        state.NewLine();

        var instructionsWithProperties = instructions
            .Where(x => x.HasProperty)
            .Where(x => !(x.Name == "ByteSize" && x.TypeInfo.PropertyType == "int")) // ByteSize is only set on deserialization so it is ignored in equality checks
            .ToList();

        if (instructionsWithProperties.Count > 0)
        {
            state.Return(endStatement: false);

            var indentedFurther = false;
            var memberIndex = 0;
            foreach (var inst in instructionsWithProperties)
            {
                if (memberIndex == 1)
                {
                    state.IncreaseIndent();
                    indentedFurther = true;
                }

                if (memberIndex != 0)
                {
                    state.Text("&& ", indented: true);
                }

                inst.GenerateEquals(state, "rhs");

                if (memberIndex != instructionsWithProperties.Count - 1)
                {
                    state.NewLine();
                }

                memberIndex++;
            }

            state.Text(";", indented: false);
            state.NewLine();

            if (indentedFurther)
            {
                state.DecreaseIndent();
            }
        }
        else
        {
            state.Return("true");
        }

        state.EndBlock();
    }

    private static void GenerateGetHashCode(GeneratorState state, string typeName, List<IProtocolInstruction> instructions)
    {
        state.MethodDeclaration(
                GeneratorState.Visibility.Public, "override int", "GetHashCode", new List<(string, string)>()
            );
        state.BeginBlock();

        if (instructions.Count(x => x.HasProperty) > 0)
        {
            state.Text("unchecked", indented: true);
            state.NewLine();
            state.BeginBlock();

            state.Text("int hash = 17;", indented: true);
            state.NewLine();

            foreach (var inst in instructions.Where(x => x.HasProperty))
            {
                state.Text($"hash = hash * 23 + {inst.Name}{(inst.TypeInfo.IsNullable ? "?" : string.Empty)}", indented: true);
                state.MethodInvocation("GetHashCode");
                state.Text($"{(inst.TypeInfo.IsNullable ? " ?? 0" : string.Empty)};", indented: false);
                state.NewLine();
            }

            state.Return("hash");
            state.EndBlock();
        }
        else
        {
            state.Return("GetType().GetHashCode()");
        }

        state.EndBlock();
    }

    private static List<IProtocolInstruction> Flatten(IReadOnlyList<IProtocolInstruction> instructions)
    {
        var retList = new List<IProtocolInstruction>();
        for (int i = 0; i < instructions.Count; i++)
        {
            if (instructions[i].Instructions.Count > 0)
                retList.AddRange(Flatten(instructions[i].Instructions));
            else
                retList.Add(instructions[i]);
        }
        return retList;
    }

    // Instructions without a property have nowhere else to document their comments, so they go on the containing type.
    private static List<string> GetInstructionNotes(IEnumerable<IProtocolInstruction> instructions)
    {
        var flattened = FlattenChunked(instructions);
        var notes = new List<string>();
        for (var i = 0; i < flattened.Count; i++)
        {
            var instruction = flattened[i];
            if (!instruction.HasProperty && instruction is not FieldInstruction && !string.IsNullOrWhiteSpace(instruction.Comment))
                notes.Add($"{DescribeInstruction(flattened, i)}: {instruction.Comment.Trim()}");
        }
        return notes;
    }

    private static List<IProtocolInstruction> FlattenChunked(IEnumerable<IProtocolInstruction> instructions)
    {
        var flattened = new List<IProtocolInstruction>();
        foreach (var instruction in instructions)
        {
            flattened.Add(instruction);

            if (instruction is ChunkedInstruction)
                flattened.AddRange(FlattenChunked(instruction.Instructions));
        }
        return flattened;
    }

    // Each note starts with a description of its instruction, since the note is no longer next to said instruction.
    // e.g. "The dummy char after <see cref="X"/> (always 0)"
    private static string DescribeInstruction(IReadOnlyList<IProtocolInstruction> instructions, int index)
    {
        var instruction = instructions[index];
        var subject = instruction switch
        {
            DummyInstruction => $"The dummy {instruction.TypeInfo.ProtocolTypeName}",
            BreakInstruction => "The break byte",
            ChunkedInstruction => "The chunked section",
            _ => throw new InvalidOperationException($"Unexpected instruction type {instruction.GetType().Name} without a property"),
        };

        var position = DescribeInstructionPosition(instructions, index);
        if (!string.IsNullOrEmpty(position))
            subject += $" {position}";

        // Dummies are named after their formatted content
        if (instruction is DummyInstruction && !string.IsNullOrWhiteSpace(instruction.Name))
            subject += $" (always {instruction.Name})";

        return subject;
    }

    private static string DescribeInstructionPosition(IReadOnlyList<IProtocolInstruction> instructions, int index)
    {
        static bool IsPublicMember(IProtocolInstruction instruction) => instruction.HasProperty && instruction is not LengthInstruction;

        var previous = instructions.Take(index).LastOrDefault(IsPublicMember);
        if (previous != null)
            return $"after <see cref=\"{previous.Name}\"/>";

        var next = instructions.Skip(index + 1).FirstOrDefault(IsPublicMember);
        return next != null ? $"before <see cref=\"{next.Name}\"/>" : string.Empty;
    }

    private static void ApplyChunked(IReadOnlyList<object> instructions, bool isChunked = false)
    {
        foreach (var inst in instructions)
        {
            if (inst is ProtocolBaseInstruction baseInst)
            {
                baseInst.IsChunked = isChunked;
            }

            if (inst is ProtocolChunkedInstruction pci)
            {
                pci.IsChunked = true;
                ApplyChunked(pci.Instructions, true);
            }
            else if (inst is ProtocolSwitchInstruction psi)
            {
                psi.IsChunked = isChunked;
                foreach (var c in psi.Cases)
                {
                    c.IsChunked = isChunked;
                    ApplyChunked(c.Instructions, isChunked);
                }
            }
        }
    }

    private static void AssociateLengths(IReadOnlyList<object> instructions)
    {
        var flattened = instructions.FlattenChunked();
        var lengths = flattened.OfType<ProtocolLengthInstruction>().ToList();

        foreach (var inst in flattened)
        {
            if (inst is ProtocolFieldInstruction pfi)
            {
                AssociateLength(pfi.Length, pfi.Name, lengths, isArray: false);
            }
            else if (inst is ProtocolArrayInstruction pai)
            {
                AssociateLength(pai.Length, pai.Name, lengths, isArray: true);
            }
            else if (inst is ProtocolSwitchInstruction psi)
            {
                foreach (var c in psi.Cases)
                {
                    AssociateLengths(c.Instructions);
                }
            }
        }

        static void AssociateLength(string length, string instName, IReadOnlyList<ProtocolLengthInstruction> lengths, bool isArray)
        {
            if (string.IsNullOrWhiteSpace(length) || int.TryParse(length, out var _))
                return;

            var matchingLength = lengths.SingleOrDefault(x => x.Name.Equals(length));
            if (matchingLength == null)
            {
                throw new InvalidOperationException($"Instruction {instName} references length {length} that was not found in set of length fields: [{string.Join(",", lengths.Select(x => x.Name))}]");
            }

            matchingLength.LengthFor = instName;
            matchingLength.LengthForArray = isArray;
        }
    }
}
