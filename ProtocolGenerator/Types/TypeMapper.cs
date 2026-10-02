using System;
using System.Collections.Generic;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Types;

public class TypeMapper
{
    private readonly Dictionary<string, ProtocolEnum> _enums = new();
    private readonly Dictionary<string, ProtocolStruct> _structs = new();

    public bool HasEnum(string enumName) => _enums.ContainsKey(enumName);

    public bool HasStruct(string structName) => _structs.ContainsKey(structName);

    public bool RegisterEnum(ProtocolEnum @enum)
    {
        if (HasEnum(@enum.Name))
            return false;

        _enums.Add(@enum.Name, @enum);
        return true;
    }

    public bool RegisterStruct(string structName, ProtocolStruct @struct)
    {
        if (HasStruct(structName))
            return false;

        _structs.Add(structName, @struct);
        return true;
    }

    public string GetEnum(string enumName) => _enums[enumName].Type;

    public IReadOnlyList<ProtocolEnumValue> GetEnumValues(string enumName) => _enums[enumName].Values ?? new List<ProtocolEnumValue>();

    public ProtocolStruct GetStruct(string structName) => _structs[structName];
}