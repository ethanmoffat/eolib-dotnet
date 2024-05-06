using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class LengthInstruction : BaseInstruction
{
    private readonly Xml.ProtocolLengthInstruction _xmlLengthInstruction;

    public override bool HasProperty => !string.IsNullOrWhiteSpace(_xmlLengthInstruction.Name);

    public LengthInstruction(Xml.ProtocolLengthInstruction xmlLengthInstruction)
    {
        _xmlLengthInstruction = xmlLengthInstruction;

        TypeInfo = new TypeInfo(
            _xmlLengthInstruction.Type,
            optional: _xmlLengthInstruction.Optional.HasValue && _xmlLengthInstruction.Optional.Value
        );

        Name = NameOrContent(_xmlLengthInstruction.Name, string.Empty);
        Comment = _xmlLengthInstruction.Comment;

        Offset = _xmlLengthInstruction.Offset ?? 0;
    }
}
