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
}
