using System.Collections.Generic;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Model.Protocol;

public class LengthInstruction : IProtocolInstruction
{
    public LengthInstruction(Xml.ProtocolLengthInstruction xmlLengthInstruction)
    {

    }

    public List<ProtocolStruct> GetNestedTypes()
    {
        return new List<ProtocolStruct>();
    }

    public void GenerateProperty(GeneratorState state)
    {
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
