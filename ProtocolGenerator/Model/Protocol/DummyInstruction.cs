using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class DummyInstruction : BaseInstruction
{
    public override bool HasProperty => false;

    public DummyInstruction(Xml.ProtocolDummyInstruction xmlDummyInstruction, EnumTypeMapper mapper)
        : base(mapper)
    {
        EoType = xmlDummyInstruction.Type.ToEoType();
        Name = NameOrContent(string.Empty, xmlDummyInstruction.Content);
    }
}
