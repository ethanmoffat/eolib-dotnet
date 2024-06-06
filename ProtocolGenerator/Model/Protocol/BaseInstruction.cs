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

    protected virtual bool IsReadOnly => false;

    protected virtual bool DeserializeToLocal => false;

    public List<IProtocolInstruction> Instructions { get; protected set; } = new();

    public virtual List<ProtocolStruct> GetNestedTypes() => new();

    public virtual void GenerateProperty(GeneratorState state)
    {
        if (!HasProperty)
            return;

        GenerateProperty(state, defaultValue: string.Empty);
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

    public virtual void GenerateDeserialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        if (TypeInfo.Optional)
        {
            state.Text($"if (reader.Remaining > 0)", indented: true);
            state.NewLine();
            state.BeginBlock();
        }

        if (TypeInfo.IsArray && HasProperty)
        {
            state.Text($"{Name}", indented: true);
            state.MethodInvocation("Add", GetDefaultValueForDeserialize());
            state.Text(";", indented: false);
            state.NewLine();
        }

        if (!TypeInfo.IsEnum && TypeInfo.EoType.HasFlag(EoType.Struct))
        {
            state.Text(Name, indented: true);
            if (TypeInfo.IsArray)
            {
                state.Text("[ndx]", indented: false);
            }
            state.MethodInvocation("Deserialize", "reader");
        }
        else
        {
            var parameters = new List<string>(2);
            if (TypeInfo.EoType.HasFlag(EoType.Padded))
            {
                if (TypeInfo.EoType.HasFlag(EoType.Fixed))
                {
                    parameters.Add(GetLengthExpression(Length, outerInstructions));
                }

                parameters.Add("true");
            }
            else if (TypeInfo.EoType.HasFlag(EoType.Blob))
            {
                parameters.Add("reader.Remaining");
            }
            else if (TypeInfo.EoType.HasFlag(EoType.Fixed))
            {
                parameters.Add(GetLengthExpression(Length, outerInstructions));
            }

            // Deserialize to "Name", with extras:
            // - If DeserializeToLocal, this is a <length> element for an array or string: *just* for deserialize, we store the result in a local var (length is implicitly mapped to another property's size for serialize)
            // - If ReadOnly, don't assign to Name; just deserialize the expected number of bytes
            // - If the field is an array, it requires an index into the array for the single element
            // - If the field is an enum, the result of the read requires a cast from int
            // - If the field is a boolean, it requires a conversion to bool; zero for false and nonzero for true
            // - If the field has an offset, it requires adjustment based on the provided offset value
            // - If the field doesn't have an associated property, ignore the value
            var preDeserialize = DeserializeToLocal
                ? $"var {Name} = "
                : HasProperty && !IsReadOnly
                    ? string.Format($"{Name}{{0}} = {{1}}",
                        $"{(TypeInfo.IsArray ? "[ndx]" : string.Empty)}",
                        $"{(TypeInfo.IsEnum ? $"({TypeInfo.PropertyType})" : string.Empty)}")
                    : string.Empty;

            var postDeserialize = HasProperty && (!IsReadOnly || DeserializeToLocal)
                ? string.Format("{0}{1}",
                    $"{(TypeInfo.EoType.HasFlag(EoType.Bool) ? " != 0" : string.Empty)}",
                    $"{(Offset != 0 ? $" + {Offset}" : string.Empty)}")
                : string.Empty;

            state.Text($"{preDeserialize}reader", indented: true);
            state.MethodInvocation(TypeInfo.GetDeserializeMethodName(), parameters.ToArray());
            state.Text(postDeserialize, indented: false);
        }

        state.Text(";", indented: false);
        state.NewLine();

        if (TypeInfo.Optional)
        {
            state.EndBlock();
        }
    }

    public virtual void GenerateToString(GeneratorState state)
    {
        if (!HasProperty)
            return;

        state.Text($"$\"{{nameof({Name})}}={{{(TypeInfo.IsNullable ? $"({Name} == null ? \"<null>\" : $\"{{{Name}}}\")" : Name)}}}\"", indented: false);
    }

    public virtual void GenerateEquals(GeneratorState state, string rhsIdentifier)
    {
        if (!HasProperty)
            return;

        state.Text($"{Name}{(TypeInfo.IsNullable ? $" == null ? {rhsIdentifier}.{Name} == null : {Name}" : string.Empty)}", indented: false);
        state.MethodInvocation("Equals", $"{rhsIdentifier}.{Name}");
    }

    protected string NameOrContent(string instructionName, string instructionContent)
    {
        if (!HasProperty && !string.IsNullOrWhiteSpace(instructionContent))
        {
            return FormatContent(instructionContent);
        }

        return IdentifierConverter.SnakeCaseToPascalCase(instructionName);
    }

    protected string FormatContent(string instructionContent)
    {
        return TypeInfo.EoType.HasFlag(EoType.String)
            ? $"\"{instructionContent}\""
            : instructionContent;
    }

    protected virtual void GenerateProperty(GeneratorState state, string defaultValue)
    {
        state.Comment(Comment);
        state.Property(GeneratorState.Visibility.Public, $"{TypeInfo.PropertyType}", Name, newLine: false);
        state.Text(" ", indented: false);
        state.BeginBlock(newLine: false, indented: false);
        state.Text(" ", indented: false);
        state.AutoGet(GeneratorState.Visibility.None, newLine: false, indented: false);
        if (!IsReadOnly)
        {
            state.Text(" ", indented: false);
            state.AutoSet(GeneratorState.Visibility.None, newLine: false, indented: false);
        }
        state.Text(" ", indented: false);
        state.EndBlock(newLine: false, indented: false);

        var needsInitialization = TypeInfo.EoType.HasFlag(EoType.Struct) && !TypeInfo.IsArray && !TypeInfo.IsInterface && !TypeInfo.IsEnum;
        if (string.IsNullOrWhiteSpace(defaultValue) && needsInitialization)
        {
            defaultValue = $"new {TypeInfo.PropertyType}()";
        }

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            state.Text($" = {defaultValue};", indented: false);
        }
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

    protected void AssertLength(GeneratorState state, string instructionLength)
    {
        var lengthIsConstant = int.TryParse(instructionLength, out var _);
        if (lengthIsConstant && !IsReadOnly)
        {
            var isPadded = TypeInfo.EoType.HasFlag(EoType.Padded);
            var op = isPadded ? ">" : "!=";
            var errorExtra = isPadded ? "no more than " : string.Empty;

            var countProperty = TypeInfo.PropertyType.StartsWith("List")
                ? "Count"
                : "Length";

            state.Text($"if ({Name}?.{countProperty} {op} {instructionLength})", indented: true);
            state.NewLine();
            state.BeginBlock();
            state.Text($"throw new InvalidOperationException($\"Expected {Name} to have {errorExtra}{instructionLength} items, but was {{({Name}?.{countProperty} ?? 0)}}\");", indented: true);
            state.NewLine();
            state.EndBlock();
        }
    }

    private string GetDefaultValueForDeserialize()
    {
        if (TypeMapper.Instance.HasEnum(TypeInfo.ProtocolTypeName))
            return $"({TypeInfo.ProtocolTypeName})0";

        if (TypeMapper.Instance.HasStruct(TypeInfo.ProtocolTypeName))
            return $"new {TypeInfo.ProtocolTypeName}()";

        return TypeInfo.EoType.HasFlag(EoType.String)
            ? "string.Empty"
            : "0";
    }
}