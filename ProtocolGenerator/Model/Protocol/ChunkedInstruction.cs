using System.Collections.Generic;
using System.Linq;

namespace ProtocolGenerator.Model.Protocol;

public class ChunkedInstruction : BaseInstruction
{
    private readonly Xml.ProtocolChunkedInstruction _xmlChunkedInstruction;
    private readonly IReadOnlyList<IProtocolInstruction> _transformed;

    public ChunkedInstruction(Xml.ProtocolChunkedInstruction xmlChunkedInstruction)
    {
        _xmlChunkedInstruction = xmlChunkedInstruction;
        _transformed = _xmlChunkedInstruction.Instructions.Select(ProtocolInstructionFactory.Transform).ToList();
    }

    public override List<Xml.ProtocolStruct> GetNestedTypes()
    {
        var nestedTypes = new List<Xml.ProtocolStruct>();
        foreach (var i in _transformed.OfType<SwitchInstruction>())
        {
            nestedTypes.AddRange(i.GetNestedTypes());
        }
        return nestedTypes;
    }

    public override void GenerateProperty(GeneratorState state)
    {
        foreach (var inst in _transformed)
        {
            inst.GenerateProperty(state);
            state.NewLine();
        }
    }
}
