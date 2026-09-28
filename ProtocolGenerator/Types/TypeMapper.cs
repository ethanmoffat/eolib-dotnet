using System;
using System.Collections.Generic;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Types;

public class TypeMapper
{
    private readonly Dictionary<string, string> _enums = new();
    private readonly Dictionary<string, ProtocolStruct> _structs = new();

    public bool HasEnum(string enumName) => _enums.ContainsKey(enumName);

    public bool HasStruct(string structName) => _structs.ContainsKey(structName);

    public bool RegisterEnum(string enumName, string enumType)
    {
        if (HasEnum(enumName))
            return false;

        _enums.Add(enumName, enumType);
        return true;
    }

    public bool RegisterStruct(string structName, ProtocolStruct @struct)
    {
        if (HasStruct(structName))
            return false;

        _structs.Add(structName, @struct);
        return true;
    }

    public string GetEnum(string enumName) => _enums[enumName];

    public ProtocolStruct GetStruct(string structName) => _structs[structName];
}