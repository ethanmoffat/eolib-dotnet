using System;
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
        if (!_isChunked)
        {
            throw new InvalidOperationException("Break bytes must be within a chunked instruction");
        }

        state.Text("writer", indented: true);
        state.MethodInvocation("AddByte", "0xFF");
        state.Text(";", indented: false);
        state.NewLine();
    }

    public override void GenerateDeserialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        if (!_isChunked)
        {
            throw new InvalidOperationException("Break bytes must be within a chunked instruction");
        }

        state.NewLine();
        state.Text("reader", indented: true);
        state.MethodInvocation("NextChunk");
        state.Text(";", indented: false);
        state.NewLine();
        state.NewLine();
    }
}
