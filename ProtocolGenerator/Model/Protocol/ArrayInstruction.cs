using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class ArrayInstruction : IProtocolInstruction
{
    private readonly Xml.ProtocolArrayInstruction _xmlArrayInstruction;

    public ArrayInstruction(Xml.ProtocolArrayInstruction xmlArrayInstruction)
    {
        _xmlArrayInstruction = xmlArrayInstruction;
    }

    public List<Xml.ProtocolStruct> GetNestedTypes()
    {
        return new List<Xml.ProtocolStruct>();
    }

    public void GenerateProperty(GeneratorState state)
    {
        state.Comment(_xmlArrayInstruction.Comment);
        state.Property(
            GeneratorState.Visibility.Public,
            TypeConverter.GetType(_xmlArrayInstruction.Type, isArray: true),
            IdentifierConverter.SnakeCaseToPascalCase(_xmlArrayInstruction.Name)
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
