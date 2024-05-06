using System.Collections.Generic;
using System.Linq;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class SwitchInstruction : BaseInstruction
{
    private readonly Xml.ProtocolSwitchInstruction _xmlSwitchInstruction;
    private readonly string _fieldName;

    public SwitchInstruction(Xml.ProtocolSwitchInstruction xmlSwitchInstruction, EnumTypeMapper mapper)
        : base(mapper)
    {
        _xmlSwitchInstruction = xmlSwitchInstruction;

        TypeName = GetSwitchInterfaceType(_xmlSwitchInstruction.Field);
        Name = GetSwitchInterfaceMemberName(_xmlSwitchInstruction.Field);
        _fieldName = IdentifierConverter.SnakeCaseToPascalCase(_xmlSwitchInstruction.Field);
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

    public override void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        var typeNameForSwitchedField = FindTypeNameForField(outerInstructions);

        state.Switch(_fieldName);
        state.BeginBlock();

        foreach (var c in _xmlSwitchInstruction.Cases)
        {
            if (c.Instructions.Count == 0)
                continue;

            if (c.Default)
            {
                state.Default();
            }
            else
            {
                if (int.TryParse(c.Value, out var _))
                    state.Case($"({typeNameForSwitchedField}){c.Value}");
                else
                    state.Case($"{typeNameForSwitchedField}.{c.Value}");
            }

            state.IncreaseIndent();

            var caseObjectType = GetSwitchCaseName(_xmlSwitchInstruction.Field, c.Value, c.Default);
            state.Text($"if ({Name} is not {caseObjectType})", indented: true);
            state.NewLine();
            state.BeginBlock();
            state.Text($"throw new InvalidOperationException($\"Expected {_fieldName} to be {caseObjectType}, but was {{{_fieldName}.GetType().Name}}\");", indented: true);
            state.NewLine();
            state.EndBlock();

            state.Text($"{Name}.Serialize(writer);", indented: true);
            state.NewLine();

            state.Break();
            state.DecreaseIndent();
        }

        state.EndBlock();
    }

    private string FindTypeNameForField(IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        return outerInstructions.Single(x => x.Name == _fieldName).TypeName;
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
