using System.Collections.Generic;

namespace ProtocolGenerator.Model.Protocol;

public class BreakInstruction : BaseInstruction
{
    private readonly bool _isChunked;

    public BreakInstruction(Xml.ProtocolBreakInstruction protocolBreakInstruction)
    {
        _isChunked = protocolBreakInstruction.IsChunked;
    }

    public override void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        state.Text("writer", indented: true);
        state.MethodInvocation("AddByte", "0xFF");
        state.Text(";", indented: false);
        state.NewLine();
    }

    public override void GenerateDeserialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        if (_isChunked)
        {
            state.NewLine();
            state.Text("reader", indented: true);
            state.MethodInvocation("NextChunk");
            state.Text(";", indented: false);
            state.NewLine();
            state.NewLine();
        }
        else
        {
            state.Text("if (reader", indented: true);
            state.MethodInvocation("GetByte");
            state.Text(" != 0xFF)", indented: false);
            state.NewLine();
            state.BeginBlock();
            state.Text("throw new InvalidOperationException(\"Missing expected break byte\");", indented: true);
            state.EndBlock();
            state.NewLine();
        }
    }
}
