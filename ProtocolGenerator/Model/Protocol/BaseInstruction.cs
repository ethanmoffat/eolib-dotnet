using System;
using System.Collections.Generic;
using System.Linq;
using ProtocolGenerator.Model.Xml;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public abstract class BaseInstruction : IProtocolInstruction
{
    public string Name { get; protected set; } = string.Empty;

    public TypeInfo TypeInfo { get; protected set; }

    public string Comment { get; protected set; } = string.Empty;

    public string Length { get; protected set; } = string.Empty;

    public int Offset { get; protected set; }

    public virtual bool HasProperty => !string.IsNullOrWhiteSpace(Name);

    public List<IProtocolInstruction> Instructions { get; protected set; } = new();

    public virtual List<ProtocolStruct> GetNestedTypes() => new();

    public virtual void GenerateProperty(GeneratorState state)
    {
        if (!HasProperty)
            return;

        state.Comment(Comment);
        state.Property(GeneratorState.Visibility.Public, $"{TypeInfo.PropertyType}", Name, newLine: false);
        state.Text(" ", indented: false);
        state.BeginBlock(newLine: false, indented: false);
        state.Text(" ", indented: false);
        state.AutoGet(GeneratorState.Visibility.None, newLine: false, indented: false);
        state.Text(" ", indented: false);
        state.AutoSet(GeneratorState.Visibility.None, newLine: false, indented: false);
        state.Text(" ", indented: false);
        state.EndBlock(newLine: false, indented: false);
    }

    public virtual void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        if (TypeInfo.Optional)
        {
            state.Text($"if ({Name}.HasValue)", indented: true);
            state.NewLine();
            state.BeginBlock();
        }

        if (!TypeInfo.IsEnum && TypeInfo.EoType.HasFlag(EoType.Struct))
        {
            state.Text(Name, indented: true);
            if (TypeInfo.IsArray)
                state.Text("[ndx]", indented: false);
            state.MethodInvocation("Serialize", "writer");
        }
        else
        {
            var parameters = new List<string>(3)
            {
                // Serialize "Name", with extras:
                // - If the field is an enum, it requires a cast to (int) prior to the call to Serialize
                // - If the field is Optional, it is Nullable and requires a call to .Value
                // - If the field is an array, it requires an index into the array for the single element
                // - If the field is a boolean, it requires a conversion to int; 1 for true and 0 for false
                // - If the field has an offset, it requires adjustment based on the provided offset value
                string.Format($"{{0}}{Name}{{1}}{{2}}{{3}}{{4}}",
                    $"{(TypeInfo.IsEnum ? "(int)" : string.Empty)}",
                    $"{(TypeInfo.Optional ? ".Value" : string.Empty)}",
                    $"{(TypeInfo.IsArray ? "[ndx]" : string.Empty)}",
                    $"{(TypeInfo.EoType.HasFlag(EoType.Bool) ? " ? 1 : 0" : string.Empty)}",
                    $"{(Offset != 0 ? $" + {-Offset}" : string.Empty)}"
                )
            };

            if (TypeInfo.EoType.HasFlag(EoType.Padded))
            {
                if (TypeInfo.EoType.HasFlag(EoType.Fixed))
                {
                    parameters.Add(GetLengthExpression(Length, outerInstructions));
                }

                parameters.Add("true");
            }
            else if (TypeInfo.EoType.HasFlag(EoType.Fixed))
            {
                parameters.Add(GetLengthExpression(Length, outerInstructions));
            }

            state.Text("writer", indented: true);
            state.MethodInvocation(TypeInfo.GetSerializeMethodName(), parameters.ToArray());
        }

        state.Text(";", indented: false);
        state.NewLine();

        if (TypeInfo.Optional)
        {
            state.EndBlock();
        }
    }

    public virtual void GenerateDeserialize(GeneratorState state) { }

    public virtual void GenerateToString(GeneratorState state)
    {
        if (!HasProperty)
            return;

        state.Text($"$\"{{nameof({Name})}}={{{Name}}}\"", indented: false);
    }

    public virtual void GenerateEquals(GeneratorState state, string rhsIdentifier)
    {
        if (!HasProperty)
            return;

        state.Text($"{Name}", indented: false);
        state.MethodInvocation("Equals", $"{rhsIdentifier}.{Name}");
    }

    protected string NameOrContent(string instructionName, string instructionContent)
    {
        if (!HasProperty && !string.IsNullOrWhiteSpace(instructionContent))
        {
            if (TypeInfo.EoType.HasFlag(EoType.String))
                return $"\"{instructionContent}\"";
            else
                return instructionContent;
        }

        return IdentifierConverter.SnakeCaseToPascalCase(instructionName);
    }

    protected static string GetLengthExpression(string instructionLength, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        if (int.TryParse(instructionLength, out var _))
            return instructionLength;

        var convertedName = IdentifierConverter.SnakeCaseToPascalCase(instructionLength);
        if (!outerInstructions.OfType<LengthInstruction>().Any(x => x.Name == convertedName))
            throw new InvalidOperationException($"Could not find 'length' instruction with name {instructionLength}");

        return convertedName;
    }
}