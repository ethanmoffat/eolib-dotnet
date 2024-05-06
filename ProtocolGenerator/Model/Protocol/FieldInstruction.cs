using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class FieldInstruction : BaseInstruction
{
    private readonly Xml.ProtocolFieldInstruction _xmlFieldInstruction;

    public override bool HasProperty => !string.IsNullOrWhiteSpace(_xmlFieldInstruction.Name);

    public FieldInstruction(Xml.ProtocolFieldInstruction xmlFieldInstruction, EnumTypeMapper mapper)
        : base(mapper)
    {
        _xmlFieldInstruction = xmlFieldInstruction;

        Comment = _xmlFieldInstruction.Comment;
        TypeName = TypeConverter.GetType(_xmlFieldInstruction.Type, isArray: false);
        RawType = _xmlFieldInstruction.Type;

        var padded = _xmlFieldInstruction.Padded.HasValue && _xmlFieldInstruction.Padded.Value;
        EoType = mapper.Has(TypeConverter.GetTypeName(RawType))
            ? mapper[TypeConverter.GetTypeName(RawType)].ToEoType()
            : _xmlFieldInstruction.Type.ToEoType(padded, !string.IsNullOrWhiteSpace(_xmlFieldInstruction.Length));

        Name = NameOrContent(_xmlFieldInstruction.Name, _xmlFieldInstruction.Content);
        Optional = _xmlFieldInstruction.Optional.HasValue && _xmlFieldInstruction.Optional.Value;

        Length = _xmlFieldInstruction.Length;
    }
}
