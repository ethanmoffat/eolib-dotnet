using System;

namespace ProtocolGenerator.Types;

public class TypeInfo
{
    public EoType EoType { get; }

    public string PropertyType { get; }

    public string ProtocolTypeName { get; }

    public string ProtocolTypeSize { get; }

    public bool IsArray { get; }

    public bool Optional { get; }

    public bool IsEnum { get; }

    public TypeInfo(string rawType, bool isArray = false, bool optional = false, bool padded = false, bool @fixed = false)
    {
        ProtocolTypeName = GetTypeName(rawType);
        ProtocolTypeSize = GetTypeSize(rawType);

        EoType = ProtocolTypeName.ToEoType(padded, @fixed);

        PropertyType = GetDotNetType(ProtocolTypeName, isArray, optional);
        IsArray = isArray;
        Optional = optional;
        IsEnum = EnumTypeMapper.Instance.Has(ProtocolTypeName);
    }

    public string GetSerializeMethodName()
    {
        var type = IsEnum
            ? EnumTypeMapper.Instance[ProtocolTypeName].ToEoType()
            : EoType;

        if (type.HasFlag(EoType.Primitive) && !string.IsNullOrWhiteSpace(ProtocolTypeSize))
        {
            type = ProtocolTypeSize.ToEoType();
            if (!type.HasFlag(EoType.Primitive))
                throw new ArgumentException($"typeSize ({ProtocolTypeSize}) must be a primitive type", nameof(ProtocolTypeSize));
        }

        if (type.HasFlag(EoType.Byte))
            return "AddByte";
        else if (type.HasFlag(EoType.Char) || type.HasFlag(EoType.Bool))
            return "AddChar";
        else if (type.HasFlag(EoType.Short))
            return "AddShort";
        else if (type.HasFlag(EoType.Three))
            return "AddThree";
        else if (type.HasFlag(EoType.Int))
            return "AddInt";
        else if (type.HasFlag(EoType.Bool))
            return "AddChar";
        else if (type.HasFlag(EoType.Blob))
            return "AddBytes";
        else if (type.HasFlag(EoType.String))
        {
            var ret = "Add{0}String";
            if (type.HasFlag(EoType.Encoded))
            {
                ret = string.Format(ret, "{0}Encoded");
            }

            if (type.HasFlag(EoType.Fixed))
            {
                ret = string.Format(ret, "Fixed");
            }
            else
            {
                ret = string.Format(ret, string.Empty);
            }

            return ret;
        }

        throw new ArgumentException($"unknown input type {type} (from {ProtocolTypeName})", nameof(type));
    }

    public string GetDeserializeMethodName()
    {
        var type = EoType;
        if (type.HasFlag(EoType.Primitive) && !string.IsNullOrWhiteSpace(ProtocolTypeSize))
        {
            type = ProtocolTypeSize.ToEoType();
            if (!type.HasFlag(EoType.Primitive))
                throw new ArgumentException("typeSize must be a primitive type", nameof(ProtocolTypeSize));
        }

        if (type.HasFlag(EoType.Byte))
            return "GetByte";
        else if (type.HasFlag(EoType.Char) || type.HasFlag(EoType.Bool))
            return "GetChar";
        else if (type.HasFlag(EoType.Short))
            return "GetShort";
        else if (type.HasFlag(EoType.Three))
            return "GetThree";
        else if (type.HasFlag(EoType.Int))
            return "GetInt";
        else if (type.HasFlag(EoType.Blob))
            return "GetBytes";
        else if (type.HasFlag(EoType.String))
        {
            var ret = "Get{0}String";
            if (type.HasFlag(EoType.Encoded))
            {
                ret = string.Format(ret, "{0}Encoded");
            }

            if (type.HasFlag(EoType.Fixed))
            {
                ret = string.Format(ret, "Fixed");
            }
            else
            {
                ret = string.Format(ret, string.Empty);
            }

            return ret;
        }

        throw new ArgumentException($"unknown input type {type} (from {ProtocolTypeName})", nameof(type));
    }

    public static string GetDotNetType(string inputType, bool isArray = false, bool optional = false)
    {
        var ret = inputType switch
        {
            "byte" or "char" or "short" or "three" or "int" => "int",
            "bool" => "bool",
            "blob" => "byte[]",
            "string" or "encoded_string" => "string",
            _ => inputType,
        };

        if (optional && ret != "byte[]")
            ret += "?";
        else if (isArray)
            ret = $"List<{ret}>";

        return ret;
    }

    public static string GetTypeName(string inputType)
    {
        return inputType.Contains(":")
            ? inputType.Split(':')[0]
            : inputType;
    }

    private static string GetTypeSize(string inputType)
    {
        return inputType.Contains(":")
            ? inputType.Split(':')[1]
            : string.Empty;
    }
}