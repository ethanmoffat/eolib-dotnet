using System;
using ProtocolGenerator.Model.Xml;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public static class ProtocolInstructionFactory
{
    public static IProtocolInstruction Transform(TypeMapper typeMapper, object input)
    {
        return input switch
        {
            ProtocolFieldInstruction pfi => new FieldInstruction(pfi, typeMapper),
            ProtocolArrayInstruction pai => new ArrayInstruction(pai, typeMapper),
            ProtocolLengthInstruction pli => new LengthInstruction(pli, typeMapper),
            ProtocolDummyInstruction pdi => new DummyInstruction(pdi, typeMapper),
            ProtocolSwitchInstruction psi => new SwitchInstruction(psi, typeMapper),
            ProtocolChunkedInstruction pci => new ChunkedInstruction(pci, typeMapper),
            ProtocolBreakInstruction pbi => new BreakInstruction(pbi),
            _ => throw new ArgumentException("Unexpected instruction type in protocol xml"),
        };
    }
}
