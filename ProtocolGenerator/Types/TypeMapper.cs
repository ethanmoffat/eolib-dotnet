using System;
using System.Collections.Generic;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Types;

public class TypeMapper
{
    public static TypeMapper Instance { get; } = new TypeMapper();

    private readonly Dictionary<string, string> _enums = new();
    private readonly Dictionary<string, ProtocolStruct> _structs = new();

    private TypeMapper() { }

    public bool HasEnum(string enumName) => _enums.ContainsKey(enumName);

    public bool HasStruct(string structName) => _structs.ContainsKey(structName);

    public void RegisterEnum(string enumName, string enumType)
    {
        if (HasEnum(enumName))
            throw new InvalidOperationException($"Duplicate enum name: {enumName} already exists in the protocol");

        _enums.Add(enumName, enumType);
    }

    public void RegisterStruct(string structName, ProtocolStruct @struct)
    {
        if (HasStruct(structName))
            throw new InvalidOperationException($"Duplicate struct name: {structName} already exists in the protocol");

        _structs.Add(structName, @struct);
    }

    public string GetEnum(string enumName) => _enums[enumName];

    public ProtocolStruct GetStruct(string structName) => _structs[structName];
}