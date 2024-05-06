using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class BreakInstruction : BaseInstruction
{
    public override void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        state.Text("writer", indented: true);
        state.MethodInvocation("AddByte", "0xFF");
        state.Text(";", indented: false);
        state.NewLine();
    }
}
