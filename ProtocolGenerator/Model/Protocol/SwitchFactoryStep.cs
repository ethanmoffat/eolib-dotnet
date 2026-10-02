namespace ProtocolGenerator.Model.Protocol;

/// <summary>
/// One switch along the path of a <see cref="SwitchFactory"/>: the switch field's value, and the case data it selects.
/// </summary>
public sealed class SwitchFactoryStep
{
    /// <summary>
    /// Gets the switch field's property name, qualified with the case types it's nested in (e.g. <c>ReplyCodeDataBanned.BanType</c>).
    /// </summary>
    public string QualifiedFieldName { get; }

    /// <summary>
    /// Gets the switch field's property name (e.g. <c>BanType</c>).
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Gets the switch data property name (e.g. <c>BanTypeData</c>).
    /// </summary>
    public string DataPropertyName { get; }

    /// <summary>
    /// Gets the switch data property name, qualified with the case types it's nested in (e.g. <c>ReplyCodeDataBanned.BanTypeData</c>).
    /// </summary>
    public string QualifiedDataPropertyName { get; }

    /// <summary>
    /// Gets the expression for the switch field's value, or an empty string for a default case (the value is a parameter).
    /// </summary>
    public string CodeExpression { get; }

    /// <summary>
    /// Gets the documentation for the switch field's value.
    /// </summary>
    public string CodeDocumentation { get; }

    /// <summary>
    /// Gets the case data type, qualified with the case types it's nested in, or an empty string when the case has no data.
    /// </summary>
    public string DataTypeName { get; }

    /// <summary>
    /// Gets whether the case data is passed to the factory, instead of created by it.
    /// </summary>
    public bool DataIsParameter { get; }

    public SwitchFactoryStep(
        string qualifiedFieldName,
        string fieldName,
        string dataPropertyName,
        string qualifiedDataPropertyName,
        string codeExpression,
        string codeDocumentation,
        string dataTypeName,
        bool dataIsParameter)
    {
        QualifiedFieldName = qualifiedFieldName;
        FieldName = fieldName;
        DataPropertyName = dataPropertyName;
        QualifiedDataPropertyName = qualifiedDataPropertyName;
        CodeExpression = codeExpression;
        CodeDocumentation = codeDocumentation;
        DataTypeName = dataTypeName;
        DataIsParameter = dataIsParameter;
    }
}
