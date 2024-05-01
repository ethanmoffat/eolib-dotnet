using System.Collections.Generic;
using System.Linq;

namespace ProtocolGenerator.Model.Protocol;

public class ChunkedInstruction : IProtocolInstruction
{
    private readonly Xml.ProtocolChunkedInstruction _xmlChunkedInstruction;
    private readonly IReadOnlyList<IProtocolInstruction> _transformed;

    public ChunkedInstruction(Xml.ProtocolChunkedInstruction xmlChunkedInstruction)
    {
        _xmlChunkedInstruction = xmlChunkedInstruction;
        _transformed = _xmlChunkedInstruction.Instructions.Select(ProtocolInstructionFactory.Transform).ToList();
    }

    public List<Xml.ProtocolStruct> GetNestedTypes()
    {
        var nestedTypes = new List<Xml.ProtocolStruct>();
        foreach (var i in _transformed.OfType<SwitchInstruction>())
        {
            nestedTypes.AddRange(i.GetNestedTypes());
        }
        return nestedTypes;
    }

    public void GenerateProperty(GeneratorState state)
    {
        foreach (var inst in _transformed)
        {
            inst.GenerateProperty(state);
            state.NewLine();
        }
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
