namespace Moffat.EndlessOnline.SDK.Protocol;

/// <summary>
/// Attribute used to mark source code that has been generated
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
public class GeneratedAttribute : Attribute
{
}
