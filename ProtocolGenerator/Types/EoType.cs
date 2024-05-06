using System;

namespace ProtocolGenerator.Types;

[Flags]
public enum EoType
{
    None = 0x0000,
    Byte = 0x0001,
    Char = 0x0002,
    Short = 0x0004,
    Three = 0x0008,
    Int = 0x0010,
    Bool = 0x0020,
    Blob = 0x0040,
    Struct = 0x0080,

    /// <summary>
    /// Flag indicates type is a primitive (supported for bool).
    /// </summary>
    Primitive = 0x0100,

    /// <summary>
    /// Flag indicates type is complex (not supported for bool)
    /// </summary>
    Complex = 0x0200,

    /// <summary>
    /// Flag indicating type is a string
    /// </summary>
    String = 0x0400,
    /// <summary>
    /// Flag indicating type is a padded string
    /// </summary>
    Padded = 0x0800,
    /// <summary>
    /// Flag indicating type is a fixed string
    /// </summary>
    Fixed = 0x1000,
    /// <summary>
    /// Flag indicating type is an encoded string
    /// </summary>
    Encoded = 0x2000
}