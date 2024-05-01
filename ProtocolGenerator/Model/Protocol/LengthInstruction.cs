using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class LengthInstruction : BaseInstruction
{
    private readonly Xml.ProtocolLengthInstruction _xmlLengthInstruction;

    public LengthInstruction(Xml.ProtocolLengthInstruction xmlLengthInstruction)
    {
        _xmlLengthInstruction = xmlLengthInstruction;

        Comment = _xmlLengthInstruction.Comment;
        TypeName = TypeConverter.GetType(_xmlLengthInstruction.Type, isArray: false);
        Name = IdentifierConverter.SnakeCaseToPascalCase(_xmlLengthInstruction.Name);
    }
}
