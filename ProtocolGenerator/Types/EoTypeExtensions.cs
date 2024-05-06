using System;

namespace ProtocolGenerator.Types;

public static class EoTypeExtensions
{
    public static EoType ToEoType(this string type, bool padded = false, bool @fixed = false)
    {
        var stringType = EoType.None;
        if (padded) stringType |= EoType.Padded;
        if (@fixed) stringType |= EoType.Fixed;

        return TypeConverter.GetTypeName(type) switch
        {
            "byte" => EoType.Byte | EoType.Primitive,
            "char" => EoType.Char | EoType.Primitive,
            "short" => EoType.Short | EoType.Primitive,
            "three" => EoType.Three | EoType.Primitive,
            "int" => EoType.Int | EoType.Primitive,
            "bool" => EoType.Bool | EoType.Primitive,
            "blob" => EoType.Blob | EoType.Complex,
            "string" => EoType.String | stringType,
            "encoded_string" => EoType.String | EoType.Encoded | stringType,
            _ => EoType.Struct | EoType.Complex,
        };
    }

    public static string GetSerializeMethodName(this EoType type, string typeSize = "")
    {
        if (type.HasFlag(EoType.Primitive) && !string.IsNullOrWhiteSpace(typeSize))
        {
            type = ToEoType(typeSize);
            if (!type.HasFlag(EoType.Primitive))
                throw new ArgumentException($"typeSize ({typeSize}) must be a primitive type", nameof(typeSize));
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

        throw new ArgumentException($"unknown input type {type}", nameof(type));
    }

    public static string GetDeserializeMethodName(this EoType type, string typeSize = "")
    {
        if (type.HasFlag(EoType.Primitive) && !string.IsNullOrWhiteSpace(typeSize))
        {
            type = ToEoType(typeSize);
            if (!type.HasFlag(EoType.Primitive))
                throw new ArgumentException("typeSize must be a primitive type", nameof(typeSize));
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

        throw new ArgumentException("unknown input type", nameof(type));
    }
}
