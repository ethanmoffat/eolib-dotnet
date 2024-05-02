using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class ArrayInstruction : BaseInstruction
{
    private readonly Xml.ProtocolArrayInstruction _xmlArrayInstruction;

    public ArrayInstruction(Xml.ProtocolArrayInstruction xmlArrayInstruction)
    {
        _xmlArrayInstruction = xmlArrayInstruction;

        Comment = _xmlArrayInstruction.Comment;
        Name = IdentifierConverter.SnakeCaseToPascalCase(_xmlArrayInstruction.Name);
        TypeName = TypeConverter.GetType(_xmlArrayInstruction.Type, isArray: true);
    }

    public override void GenerateToString(GeneratorState state)
    {
        state.Text($"$\"{{nameof({Name})}}=[{{string.Join(\",\", {Name}.Select(x => x.ToString()))}}]\"", indented: false);
    }
}
