using System.Collections.Generic;
using System.Linq;

namespace ProtocolGenerator.Model.Protocol;

/// <summary>
/// A static factory method that creates a type with its switch field(s) and the matching case data set together.
/// </summary>
public sealed class SwitchFactory
{
    /// <summary>
    /// Gets the method name (e.g. <c>ForBannedTemporary</c>).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the switches set by the factory, from the outermost switch to the innermost.
    /// </summary>
    public IReadOnlyList<SwitchFactoryStep> Steps { get; }

    /// <summary>
    /// Gets the type of the switch field when the factory is for a default case (the value is a parameter), or an empty string.
    /// </summary>
    public string DefaultCodeType { get; }

    /// <summary>
    /// Gets the expressions for the values of the switch's other cases, which a default case factory rejects.
    /// </summary>
    public IReadOnlyList<string> ExcludedCodes { get; }

    public bool IsDefault => !string.IsNullOrEmpty(DefaultCodeType);

    public SwitchFactoryStep Leaf => Steps.Last();

    public SwitchFactory(string name, IReadOnlyList<SwitchFactoryStep> steps, string defaultCodeType = "", IReadOnlyList<string> excludedCodes = null)
    {
        Name = name;
        Steps = steps;
        DefaultCodeType = defaultCodeType;
        ExcludedCodes = excludedCodes ?? new List<string>();
    }
}
