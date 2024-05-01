using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class FieldInstruction : IProtocolInstruction
{
    private readonly Xml.ProtocolFieldInstruction _xmlFieldInstruction;

    public FieldInstruction(Xml.ProtocolFieldInstruction xmlFieldInstruction)
    {
        _xmlFieldInstruction = xmlFieldInstruction;
    }

    public List<Xml.ProtocolStruct> GetNestedTypes()
    {
        return new List<Xml.ProtocolStruct>();
    }

    public void GenerateProperty(GeneratorState state)
    {
        if (string.IsNullOrWhiteSpace(_xmlFieldInstruction.Name))
        {
            return;
        }

        state.Comment(_xmlFieldInstruction.Comment);
        state.Property(
            GeneratorState.Visibility.Public,
            TypeConverter.GetType(_xmlFieldInstruction.Type, isArray: false),
            IdentifierConverter.SnakeCaseToPascalCase(_xmlFieldInstruction.Name)
        );
        state.BeginBlock();
        state.AutoGet(GeneratorState.Visibility.None);
        state.AutoSet(GeneratorState.Visibility.None);
        state.EndBlock();
    }

    public void GenerateSerialize(GeneratorState state)
    {
    }

    public void GenerateDeserialize(GeneratorState state)
    {
    }

    public void GenerateToString(GeneratorState state)
    {
    }

    public void GenerateEquals(GeneratorState state)
    {
    }

    public void GenerateGetHashCode(GeneratorState state)
    {
    }
}
