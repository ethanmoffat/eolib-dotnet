using System.Collections.Generic;

namespace ProtocolGenerator.Model.Protocol;

public class DummyInstruction : IProtocolInstruction
{
    public DummyInstruction(Xml.ProtocolDummyInstruction xmlDummyInstruction)
    {
    }

    public List<Xml.ProtocolStruct> GetNestedTypes()
    {
        return new List<Xml.ProtocolStruct>();
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
