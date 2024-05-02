using System.Collections.Generic;

namespace ProtocolGenerator.Model.Protocol;

public interface IProtocolInstruction
{
    string TypeName { get; }

    string Name { get; }

    string Comment { get; }

    bool HasProperty { get; }

    List<IProtocolInstruction> Instructions { get; }

    List<Xml.ProtocolStruct> GetNestedTypes();

    void GenerateProperty(GeneratorState state);

    void GenerateSerialize(GeneratorState state);

    void GenerateDeserialize(GeneratorState state);

    void GenerateToString(GeneratorState state);

    void GenerateEquals(GeneratorState state, string rhsIdentifier);
}
