using System;
using ProtocolGenerator.Model.Xml;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public static class ProtocolInstructionFactory
{
    public static IProtocolInstruction Transform(object input, EnumTypeMapper mapper)
    {
        return input switch
        {
            ProtocolFieldInstruction pfi => new FieldInstruction(pfi, mapper),
            ProtocolArrayInstruction pai => new ArrayInstruction(pai, mapper),
            ProtocolLengthInstruction pli => new LengthInstruction(pli, mapper),
            ProtocolDummyInstruction pdi => new DummyInstruction(pdi, mapper),
            ProtocolSwitchInstruction psi => new SwitchInstruction(psi, mapper),
            ProtocolChunkedInstruction pci => new ChunkedInstruction(pci, mapper),
            ProtocolBreakInstruction => new BreakInstruction(mapper),
            _ => throw new ArgumentException("Unexpected instruction type in protocol xml"),
        };
    }
}
