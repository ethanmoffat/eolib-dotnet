using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class FieldInstruction : BaseInstruction
{
    private readonly Xml.ProtocolFieldInstruction _xmlFieldInstruction;

    public override bool HasProperty => !string.IsNullOrWhiteSpace(_xmlFieldInstruction.Name);

    protected override bool IsReadOnly => HasProperty && !string.IsNullOrWhiteSpace(_xmlFieldInstruction.Content);

    public FieldInstruction(Xml.ProtocolFieldInstruction xmlFieldInstruction)
    {
        _xmlFieldInstruction = xmlFieldInstruction;

        TypeInfo = new TypeInfo(
            _xmlFieldInstruction.Type,
            optional: _xmlFieldInstruction.Optional.HasValue && _xmlFieldInstruction.Optional.Value,
            padded: _xmlFieldInstruction.Padded.HasValue && _xmlFieldInstruction.Padded.Value,
            @fixed: !string.IsNullOrWhiteSpace(_xmlFieldInstruction.Length)
        );

        Name = NameOrContent(_xmlFieldInstruction.Name, _xmlFieldInstruction.Content);
        Comment = _xmlFieldInstruction.Comment;

        Length = _xmlFieldInstruction.Length;
    }

    public override void GenerateProperty(GeneratorState state)
    {
        if (!HasProperty)
            return;

        base.GenerateProperty(state, IsReadOnly, FormatContent(_xmlFieldInstruction.Content));
    }
}
