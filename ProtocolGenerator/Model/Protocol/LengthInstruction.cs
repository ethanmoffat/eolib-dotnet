using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class LengthInstruction : BaseInstruction
{
    private readonly Xml.ProtocolLengthInstruction _xmlLengthInstruction;

    public override bool HasProperty => !string.IsNullOrWhiteSpace(_xmlLengthInstruction.Name);

    public LengthInstruction(Xml.ProtocolLengthInstruction xmlLengthInstruction, EnumTypeMapper mapper)
        : base(mapper)
    {
        _xmlLengthInstruction = xmlLengthInstruction;

        Comment = _xmlLengthInstruction.Comment;
        TypeName = TypeConverter.GetType(_xmlLengthInstruction.Type, isArray: false);
        RawType = _xmlLengthInstruction.Type;

        EoType = _xmlLengthInstruction.Type.ToEoType();
        Name = NameOrContent(_xmlLengthInstruction.Name, string.Empty);
        Optional = _xmlLengthInstruction.Optional.HasValue && _xmlLengthInstruction.Optional.Value;

        Offset = _xmlLengthInstruction.Offset ?? 0;
    }
}
