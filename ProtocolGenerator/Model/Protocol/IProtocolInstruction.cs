using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public interface IProtocolInstruction
{
    TypeInfo TypeInfo { get; }

    string Name { get; }

    string Comment { get; }

    bool HasProperty { get; }

    List<IProtocolInstruction> Instructions { get; }

    List<Xml.ProtocolStruct> GetNestedTypes();

    void GenerateProperty(GeneratorState state);

    void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions);

    void GenerateDeserialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions);

    void GenerateToString(GeneratorState state);

    void GenerateEquals(GeneratorState state, string rhsIdentifier);
}
