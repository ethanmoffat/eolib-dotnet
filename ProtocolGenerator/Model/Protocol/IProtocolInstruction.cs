using System.Collections.Generic;

namespace ProtocolGenerator.Model.Protocol;

public interface IProtocolInstruction
{
    List<Xml.ProtocolStruct> GetNestedTypes();

    void GenerateProperty(GeneratorState state);

    void GenerateSerialize(GeneratorState state);

    void GenerateDeserialize(GeneratorState state);

    void GenerateToString(GeneratorState state);

    void GenerateEquals(GeneratorState state);

    void GenerateGetHashCode(GeneratorState state);
}
