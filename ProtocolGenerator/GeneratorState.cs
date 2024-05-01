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

    public void TypeDeclaration(Visibility visibility, ObjectType objectType, string typeName, string baseType = "")
    {
        AppendIndented($"{String(visibility)} {String(objectType)} {typeName}");
        if (!string.IsNullOrWhiteSpace(baseType))
        {
            var convertedType = TypeConverter.GetType(baseType);
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

    public void MethodDeclaration(Visibility visibility, string returnType, string methodName, List<(string, string)> parameterNamesAndTypes)
    {
        var sb = new StringBuilder();
        foreach (var p in parameterNamesAndTypes)
            sb.Append($"{p.Item1} {p.Item2}");

        AppendIndentedLine($"{String(visibility)} {returnType} {methodName}({sb})");
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

    public void Return(string returnValue = "", bool endStatement = true)
    {
        AppendIndentedLine($"return {returnValue}{(endStatement ? ";" : "")}");
    }

    public void AutoProperty(Visibility visibility, string type, string name, string impl)
    {
        var vis = String(visibility);
        if (vis.Length > 0)
            AppendIndentedLine($"{vis} {type} {name} => {impl};");
        else
            AppendIndentedLine($"{type} {name} => {impl};");
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
