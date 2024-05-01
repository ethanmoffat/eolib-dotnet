using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class FieldInstruction : BaseInstruction
{
    private readonly Xml.ProtocolFieldInstruction _xmlFieldInstruction;

    public FieldInstruction(Xml.ProtocolFieldInstruction xmlFieldInstruction)
    {
        _xmlFieldInstruction = xmlFieldInstruction;

        Comment = _xmlFieldInstruction.Comment;
        TypeName = TypeConverter.GetType(_xmlFieldInstruction.Type, isArray: false);
        Name = IdentifierConverter.SnakeCaseToPascalCase(_xmlFieldInstruction.Name ?? string.Empty);
    }
}
