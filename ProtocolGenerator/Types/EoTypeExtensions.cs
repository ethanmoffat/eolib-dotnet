namespace ProtocolGenerator.Types;

public static class EoTypeExtensions
{
    public static EoType ToEoType(this string type, bool padded = false, bool @fixed = false)
    {
        var stringType = EoType.None;
        if (padded) stringType |= EoType.Padded;
        if (@fixed) stringType |= EoType.Fixed;

        return type switch
        {
            "" => EoType.None,
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
}
