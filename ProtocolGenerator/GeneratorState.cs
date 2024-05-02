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
        None,
        Public,
        Private
    }

    public enum ObjectType
    {
        Class,
        Struct,
        Enum,
        Interface,
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
        AppendIndentedLine($"{String(visibility)} {returnType} {methodName}{ParameterList(parameterNamesAndTypes)}");
    }

    public void BeginBlock(bool newLine = true, bool indented = true)
    {
        Action<string> fn = newLine ? AppendIndentedLine : AppendIndented;
        if (!indented)
            fn = newLine ? AppendLine : Append;

        fn("{");

        IncreaseIndent();
    }

    public void EndBlock(bool newLine = true, bool indented = true)
    {
        DecreaseIndent();

        Action<string> fn = newLine ? AppendIndentedLine : AppendIndented;
        if (!indented)
            fn = newLine ? AppendLine : Append;

        fn("}");
    }

    public void IncreaseIndent() => _indent++;

    public void DecreaseIndent() => _indent = Math.Max(_indent - 1, 0);

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
        Action<string> fn = endStatement ? AppendIndentedLine : AppendIndented;
        fn($"return {returnValue}{(endStatement ? ";" : "")}");
    }

    public void Property(Visibility visibility, string type, string name, bool newLine = true, bool indented = true)
    {
        Action<string> fn = newLine ? AppendIndentedLine : AppendIndented;
        if (!indented)
            fn = newLine ? AppendLine : Append;

        var vis = String(visibility);
        if (vis.Length > 0)
            fn($"{vis} {type} {name}");
        else
            fn($"{type} {name}");
    }

    public void AutoProperty(Visibility visibility, string type, string name, string impl)
    {
        var vis = String(visibility);
        if (vis.Length > 0)
            AppendIndentedLine($"{vis} {type} {name} => {impl};");
        else
            AppendIndentedLine($"{type} {name} => {impl};");
    }

    public void AutoGet(Visibility visibility, bool newLine = true, bool indented = true)
    {
        Action<string> fn = newLine ? AppendIndentedLine : AppendIndented;
        if (!indented)
            fn = newLine ? AppendLine : Append;

        var vis = String(visibility);
        if (vis.Length > 0)
            fn($"{vis} get;");
        else
            fn($"get;");
    }

    public void AutoSet(Visibility visibility, bool newLine = true, bool indented = true)
    {
        Action<string> fn = newLine ? AppendIndentedLine : AppendIndented;
        if (!indented)
            fn = newLine ? AppendLine : Append;

        var vis = String(visibility);
        if (vis.Length > 0)
            fn($"{vis} set;");
        else
            fn($"set;");
    }

    public void NewLine() => AppendLine();

    public void MethodInvocation(string methodName, params string[] parameters)
    {
        Append($".{methodName}({string.Join(", ", parameters)})");
    }

    public void Text(string text, bool indented)
    {
        if (indented)
            AppendIndented(text);
        else
            Append(text);
    }

    private void AppendIndented(string value) => Append($"{Indent()}{value}");

    private void AppendIndentedLine(string value) => AppendIndented($"{value}\n");

    private void Append(string value) => _output.Append(value);

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

    private string ParameterList(List<(string, string)> parameterNamesAndTypes)
    {
        var sb = new StringBuilder("(");
        for (int i = 0; i < parameterNamesAndTypes.Count; i++)
        {
            if (i != 0)
                sb.Append(",");

            var p = parameterNamesAndTypes[i];
            sb.Append($"{p.Item1} {p.Item2}");
        }
        sb.Append(")");
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
            ObjectType.Interface => "interface",
            _ => "",
        };
    }
}
