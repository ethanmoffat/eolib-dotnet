using System;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Model.Protocol;

public static class ProtocolInstructionFactory
{
    public static IProtocolInstruction Transform(object input)
    {
        return input switch
        {
            ProtocolFieldInstruction pfi => new FieldInstruction(pfi),
            ProtocolArrayInstruction pai => new ArrayInstruction(pai),
            ProtocolLengthInstruction pli => new LengthInstruction(pli),
            ProtocolDummyInstruction pdi => new DummyInstruction(pdi),
            ProtocolSwitchInstruction psi => new SwitchInstruction(psi),
            ProtocolChunkedInstruction pci => new ChunkedInstruction(pci),
            ProtocolBreakInstruction => new BreakInstruction(),
            _ => throw new ArgumentException("Unexpected instruction type in protocol xml"),
        };
    }
}
