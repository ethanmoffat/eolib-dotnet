using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class FieldInstruction : BaseInstruction
{
    private readonly Xml.ProtocolFieldInstruction _xmlFieldInstruction;

    public override bool HasProperty => !string.IsNullOrWhiteSpace(_xmlFieldInstruction.Name);

    public override bool IsReadOnly => !string.IsNullOrWhiteSpace(_xmlFieldInstruction.Content);

    protected override bool StoreDeserializedValue => HasProperty;

    public FieldInstruction(Xml.ProtocolFieldInstruction xmlFieldInstruction, TypeMapper typeMapper)
    {
        _xmlFieldInstruction = xmlFieldInstruction;

        TypeInfo = new TypeInfo(
            typeMapper,
            _xmlFieldInstruction.Type,
            optional: _xmlFieldInstruction.Optional.HasValue && _xmlFieldInstruction.Optional.Value,
            padded: _xmlFieldInstruction.Padded.HasValue && _xmlFieldInstruction.Padded.Value,
            @fixed: !string.IsNullOrWhiteSpace(_xmlFieldInstruction.Length)
        );

        Name = NameOrContent(_xmlFieldInstruction.Name, _xmlFieldInstruction.Content);
        Comment = _xmlFieldInstruction.Comment;

        Length = _xmlFieldInstruction.Length;
    }

    public override void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        AssertLength(state, _xmlFieldInstruction.Length);
        base.GenerateSerialize(state, outerInstructions);
    }

    protected override void GenerateProperty(GeneratorState state, string defaultValue)
    {
        if (IsReadOnly)
        {
            var constantName = $"Default{Name}";
            state.Comment(string.IsNullOrWhiteSpace(Comment) ? $"The default value of the `{_xmlFieldInstruction.Name}` field." : Comment);
            state.Constant(GeneratorState.Visibility.Public, TypeInfo.PropertyType, constantName, FormatContent(_xmlFieldInstruction.Content));
            state.NewLine();
            base.GenerateProperty(state, constantName);
            return;
        }

        base.GenerateProperty(state, TypeInfo.Optional ? string.Empty : FormatContent(_xmlFieldInstruction.Content));
    }
}
