namespace EOLib.Protocol;

/// <summary>
/// Attribute used to mark source code that has been generated
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class GeneratedAttribute : Attribute
{
}
