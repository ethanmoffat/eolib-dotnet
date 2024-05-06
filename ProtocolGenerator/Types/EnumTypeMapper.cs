using System;
using System.Collections.Generic;

namespace ProtocolGenerator.Types;

public class EnumTypeMapper
{
    private readonly Dictionary<string, string> _types = new();

    public bool Has(string enumName) => _types.ContainsKey(enumName);

    public void Register(string enumName, string enumType)
    {
        if (Has(enumName))
            throw new InvalidOperationException($"Duplicate enum name: {enumName} already exists in the protocol");

        _types.Add(enumName, enumType);
    }

    public string this[string enumName] => _types[enumName];
}