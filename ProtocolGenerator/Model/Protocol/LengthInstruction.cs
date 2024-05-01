using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class LengthInstruction : IProtocolInstruction
{
    private readonly Xml.ProtocolLengthInstruction _xmlLengthInstruction;

    public LengthInstruction(Xml.ProtocolLengthInstruction xmlLengthInstruction)
    {
        _xmlLengthInstruction = xmlLengthInstruction;
    }

    public List<Xml.ProtocolStruct> GetNestedTypes()
    {
        return new List<Xml.ProtocolStruct>();
    }

    public void GenerateProperty(GeneratorState state)
    {
        state.Comment(_xmlLengthInstruction.Comment);
        state.Property(
            GeneratorState.Visibility.Public,
            TypeConverter.GetType(_xmlLengthInstruction.Type, isArray: false),
            IdentifierConverter.SnakeCaseToPascalCase(_xmlLengthInstruction.Name)
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
