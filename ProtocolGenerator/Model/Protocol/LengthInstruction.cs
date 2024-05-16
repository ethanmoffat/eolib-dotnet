using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class LengthInstruction : BaseInstruction
{
    private readonly Xml.ProtocolLengthInstruction _xmlLengthInstruction;

    public override bool HasProperty => !string.IsNullOrWhiteSpace(_xmlLengthInstruction.Name);

    protected override bool IsReadOnly => !string.IsNullOrWhiteSpace(_xmlLengthInstruction.LengthFor);

    protected override bool DeserializeToLocal => IsReadOnly;

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

    public override void GenerateProperty(GeneratorState state)
    {
        if (IsReadOnly)
        {
            var lengthFor = IdentifierConverter.SnakeCaseToPascalCase(_xmlLengthInstruction.LengthFor);
            var useCount = _xmlLengthInstruction.LengthForArray;

            state.Comment(Comment);
            state.Property(GeneratorState.Visibility.Private, $"{TypeInfo.PropertyType}", Name, newLine: false);
            state.Text($" => {lengthFor}.{(useCount ? "Count" : "Length")};", indented: false);
        }
        else
        {
            base.GenerateProperty(state);
        }
    }
}
