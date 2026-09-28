using System.Collections.Generic;
using System.Linq;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class ChunkedInstruction : BaseInstruction
{
    public ChunkedInstruction(Xml.ProtocolChunkedInstruction xmlChunkedInstruction, TypeMapper typeMapper)
    {
        Instructions = xmlChunkedInstruction.Instructions.Select(x => ProtocolInstructionFactory.Transform(typeMapper, x)).ToList();
    }

    public override List<Xml.ProtocolStruct> GetNestedTypes()
    {
        var nestedTypes = new List<Xml.ProtocolStruct>();
        foreach (var i in Instructions.OfType<SwitchInstruction>())
        {
            nestedTypes.AddRange(i.GetNestedTypes());
        }
        return nestedTypes;
    }

    public override void GenerateProperty(GeneratorState state)
    {
        foreach (var inst in Instructions)
        {
            inst.GenerateProperty(state);

            if (inst.HasProperty)
            {
                state.NewLine();
                state.NewLine();
            }
        }
    }

    public override void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        state.Text("writer.StringSanitization = true;", indented: true);
        state.NewLine();

        foreach (var inst in Instructions)
            inst.GenerateSerialize(state, outerInstructions);

        state.Text("writer.StringSanitization = false;", indented: true);
        state.NewLine();
    }

    public override void GenerateDeserialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        state.Text("reader.ChunkedReadingMode = true;", indented: true);
        state.NewLine();

        foreach (var inst in Instructions)
            inst.GenerateDeserialize(state, outerInstructions);

        state.Text("reader.ChunkedReadingMode = false;", indented: true);
        state.NewLine();
    }
}
