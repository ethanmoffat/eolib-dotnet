using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class DummyInstruction : BaseInstruction
{
    public override bool HasProperty => false;

    public DummyInstruction(Xml.ProtocolDummyInstruction xmlDummyInstruction)
    {
        TypeInfo = new TypeInfo(xmlDummyInstruction.Type);
        Name = NameOrContent(string.Empty, xmlDummyInstruction.Content);
    }

    public override void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        if (outerInstructions.Count > 2)
        {
            state.Text("if (oldWriterLength == writer.Length)", indented: true);
            state.NewLine();
            state.BeginBlock();
        }

        base.GenerateSerialize(state, outerInstructions);

        if (outerInstructions.Count > 2)
        {
            state.EndBlock();
        }
    }

    public override void GenerateDeserialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        if (outerInstructions.Count > 2)
        {
            state.Text("if (reader.Position == readerStartPosition)", indented: true);
            state.NewLine();
            state.BeginBlock();
        }

        base.GenerateDeserialize(state, outerInstructions);

        if (outerInstructions.Count > 2)
        {
            state.EndBlock();
        }
    }
}
