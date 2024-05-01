using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class SwitchInstruction : IProtocolInstruction
{
    private readonly Xml.ProtocolSwitchInstruction _xmlSwitchInstruction;

    public SwitchInstruction(Xml.ProtocolSwitchInstruction xmlSwitchInstruction)
    {
        _xmlSwitchInstruction = xmlSwitchInstruction;
    }

    public List<Xml.ProtocolStruct> GetNestedTypes()
    {
        var switchInterfaceName = GetSwitchInterfaceType(_xmlSwitchInstruction.Field);
        var nestedTypes = new List<Xml.ProtocolStruct>
        {
            new Xml.ProtocolStruct
            {
                Name = switchInterfaceName,
                Instructions = new List<object>(),
                IsInterface = true,
            }
        };

        foreach (var c in _xmlSwitchInstruction.Cases)
        {
            if (c.Instructions.Count == 0)
                continue;

            nestedTypes.Add(new Xml.ProtocolStruct
            {
                Name = GetSwitchCaseName(_xmlSwitchInstruction.Field, c.Value, c.Default),
                Comment = c.Comment,
                Instructions = c.Instructions,
                BaseType = switchInterfaceName,
            });
        }

        return nestedTypes;
    }

    public void GenerateProperty(GeneratorState state)
    {
        state.Property(
            GeneratorState.Visibility.Public,
            GetSwitchInterfaceType(_xmlSwitchInstruction.Field),
            GetSwitchInterfaceMemberName(_xmlSwitchInstruction.Field)
        );
        state.BeginBlock();
        state.AutoGet(GeneratorState.Visibility.None);
        state.AutoSet(GeneratorState.Visibility.None);
        state.EndBlock();
    }

    public void GenerateSerialize(GeneratorState state)
    {
    }

    public void GenerateDeserialize(GeneratorState state)
    {
    }

    public void GenerateToString(GeneratorState state)
    {
    }

    public void GenerateEquals(GeneratorState state)
    {
    }

    public void GenerateGetHashCode(GeneratorState state)
    {
    }

    private static string GetSwitchInterfaceType(string fieldName)
    {
        var converted = IdentifierConverter.SnakeCaseToPascalCase(fieldName);
        return $"I{converted}Data";
    }

    private static string GetSwitchInterfaceMemberName(string fieldName)
    {
        var converted = IdentifierConverter.SnakeCaseToPascalCase(fieldName);
        return $"{converted}Data";
    }

    private static string GetSwitchCaseName(string switchField, string caseValue, bool isDefault)
    {
        return $"{GetSwitchInterfaceMemberName(switchField)}{(isDefault ? "Default" : caseValue)}";
    }
}
