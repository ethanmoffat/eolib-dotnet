using System;
using System.Collections.Generic;
using System.Text;
using ProtocolGenerator.Model.Xml;
using ProtocolGenerator.Types;

namespace ProtocolGenerator;

public class GeneratorState
{
    public enum Visibility
    {
        Public,
        Private
    }

    public enum ObjectType
    {
        Class,
        Struct,
        Enum
    }

    private readonly StringBuilder _output = new();
    private int _indent;

    public string Output() => _output.ToString();

    public void Comment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return;
        }

        AppendIndentedLine("/// <summary>");
        foreach (var line in comment.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            AppendIndentedLine($"/// {line.Trim()}");
        }
        AppendIndentedLine("/// </summary>");
    }

    public void Attribute(string attributeName)
    {
        AppendIndentedLine($"[{attributeName}]");
    }

    public void TypeDeclaration(Visibility visibility, ObjectType objectType, string typeName, string baseType)
    {
        AppendIndented($"{String(visibility)} {String(objectType)} {typeName}");
        if (!string.IsNullOrWhiteSpace(baseType))
        {
            var convertedType = TypeConverter.ToPrimitive(baseType);
            if (!string.IsNullOrWhiteSpace(convertedType) && convertedType != "int")
            {
                AppendLine($" : {convertedType}");
            }
            else
            {
                AppendLine();
            }
        }
        else
        {
            AppendLine();
        }
    }

    public void BeginBlock()
    {
        AppendIndentedLine("{");
        _indent++;
    }

    public void EndBlock()
    {
        _indent = Math.Max(_indent - 1, 0);
        AppendIndentedLine("}");
    }

    public void ValuesList(List<ProtocolEnumValue> values)
    {
        foreach (var value in values)
        {
            Comment(value.Comment);
            AppendIndentedLine($"{value.Name.Trim()} = {value.Value.Trim()},");
        }
    }

    private void AppendIndented(string value) => _output.Append($"{Indent()}{value}");

    private void AppendIndentedLine(string value) => AppendIndented($"{value}\n");

    private void AppendLine(string value = "") => _output.AppendLine(value);

    private string Indent()
    {
        var sb = new StringBuilder(32);
        for (int i = 0; i < _indent; i++)
        {
            sb.Append("    ");
        }
        return sb.ToString();
    }

    private static string String(Visibility vis)
    {
        return vis switch
        {
            Visibility.Public => "public",
            Visibility.Private => "private",
            _ => "",
        };
    }

    private static string String(ObjectType ot)
    {
        return ot switch
        {
            ObjectType.Enum => "enum",
            ObjectType.Class => "class",
            ObjectType.Struct => "struct",
            _ => "",
        };
    }
}
