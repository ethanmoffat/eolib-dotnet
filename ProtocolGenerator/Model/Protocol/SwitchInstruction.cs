using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class SwitchInstruction : BaseInstruction
{
    private readonly Xml.ProtocolSwitchInstruction _xmlSwitchInstruction;

    public SwitchInstruction(Xml.ProtocolSwitchInstruction xmlSwitchInstruction)
    {
        _xmlSwitchInstruction = xmlSwitchInstruction;

        TypeName = GetSwitchInterfaceType(_xmlSwitchInstruction.Field);
        Name = GetSwitchInterfaceMemberName(_xmlSwitchInstruction.Field);
    }

    public override List<Xml.ProtocolStruct> GetNestedTypes()
    {
        var nestedTypes = new List<Xml.ProtocolStruct>
        {
            new Xml.ProtocolStruct
            {
                Name = TypeName,
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
                BaseType = TypeName,
            });
        }

        return nestedTypes;
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
