using System;
using System.Collections.Generic;
using Xml = ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Types;

public class TypeInfo
{
    public EoType EoType { get; }

    public string PropertyType { get; }

    public string ProtocolTypeName { get; }

    public string ProtocolTypeSize { get; }

    public bool IsArray { get; }

    public bool IsInterface { get; }

    public bool Optional { get; }

    public bool IsEnum { get; }

    public bool IsStruct { get; }

    public bool IsNullable { get; }

    private readonly TypeMapper _typeMapper;

    public TypeInfo(TypeMapper typeMapper, string rawType, bool isArray = false, bool isInterface = false, bool optional = false, bool padded = false, bool @fixed = false)
    {
        _typeMapper = typeMapper;

        ProtocolTypeName = GetTypeName(rawType);
        ProtocolTypeSize = GetTypeSize(rawType);

        EoType = ProtocolTypeName.ToEoType(padded, @fixed);

        IsStruct = _typeMapper.HasStruct(ProtocolTypeName);
        PropertyType = GetDotNetType(ProtocolTypeName, isArray, optional, IsStruct);
        IsArray = isArray;
        IsInterface = isInterface;
        Optional = optional;
        IsEnum = _typeMapper.HasEnum(ProtocolTypeName);

        IsNullable = PropertyType.Contains("List") ||
            PropertyType.Contains("string") ||
            PropertyType.Contains("[]") ||
            PropertyType.Contains("?") ||
            IsStruct;
    }

    public string GetSerializeMethodName()
    {
        var type = IsEnum
            ? _typeMapper.GetEnum(ProtocolTypeName).ToEoType()
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
        var type = IsEnum
            ? _typeMapper.GetEnum(ProtocolTypeName).ToEoType()
            : EoType;

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

    public int CalculateByteSize()
    {
        var typeName = string.IsNullOrWhiteSpace(ProtocolTypeSize) ? ProtocolTypeName : ProtocolTypeSize;
        if (_typeMapper.HasStruct(typeName))
        {
            return CalculateByteSize(_typeMapper.GetStruct(typeName).Instructions);
        }
        else if (_typeMapper.HasEnum(typeName))
        {
            typeName = _typeMapper.GetEnum(typeName);
        }
        else if (EoType.HasFlag(EoType.Complex))
        {
            throw new InvalidOperationException($"Unable to calculate size of unknown complex type {ProtocolTypeName}");
        }

        return typeName switch
        {
            "byte" or "char" or "bool" => 1,
            "short" => 2,
            "three" => 3,
            "int" => 4,
            _ => throw new InvalidOperationException($"Unable to calculate size of unbounded complex type {typeName}")
        };
    }

    public static string GetDotNetType(string inputType, bool isArray = false, bool optional = false, bool isStruct = false)
    {
        var ret = inputType switch
        {
            "byte" or "char" or "short" or "three" or "int" => "int",
            "bool" => "bool",
            "blob" => "byte[]",
            "string" or "encoded_string" => "string",
            _ => inputType,
        };

        if (optional && ret != "byte[]" && !isStruct)
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

    public static string GetTypeSize(string inputType)
    {
        return inputType.Contains(":")
            ? inputType.Split(':')[1]
            : string.Empty;
    }

    private int CalculateByteSize(IReadOnlyList<object> instructions)
    {
        var flattenedInstructions = new List<object>();
        foreach (var inst in instructions)
        {
            if (inst is Xml.ProtocolChunkedInstruction pce)
                flattenedInstructions.AddRange(pce.Instructions);
            else
                flattenedInstructions.Add(inst);
        }

        var ret = 0;

        foreach (var inst in flattenedInstructions)
        {
            string typeName, typeSize, length;

            if (inst is Xml.ProtocolArrayInstruction pai)
            {
                typeName = GetTypeName(pai.Type);
                typeSize = GetTypeSize(pai.Type);
                length = pai.Length;
            }
            else if (inst is Xml.ProtocolFieldInstruction pfi)
            {
                typeName = GetTypeName(pfi.Type);
                typeSize = GetTypeSize(pfi.Type);
                length = pfi.Length;
            }
            else
            {
                if (inst is Xml.ProtocolBreakInstruction)
                    ret += 1;

                continue;
            }

            if (!string.IsNullOrWhiteSpace(typeSize))
                typeName = typeSize;

            if (!string.IsNullOrWhiteSpace(length))
            {
                if (!int.TryParse(length, out var lengthInt))
                    throw new ArgumentException($"Length must be a fixed size for {typeName}");

                ret += new TypeInfo(_typeMapper, typeName).CalculateByteSize() * lengthInt;
            }
            else
            {
                ret += new TypeInfo(_typeMapper, typeName).CalculateByteSize();
            }
        }

        return ret;
    }
}