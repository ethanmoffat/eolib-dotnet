namespace EOLib.Data;

/// <summary>
/// Constants for the maximum values of the EO numeric types.
/// <para/>
/// The largest valid value for each type is <c>TYPE_MAX - 1</c>.
/// </summary>
public static class EoNumericLimits
{
    /// <summary>
    /// The maximum value of an EO char (1-byte encoded integer type)
    /// </summary>
    public const int CHAR_MAX = 253;

    /// <summary>
    /// The maximum value of an EO short (2-byte encoded integer type)
    /// </summary>
    public const int SHORT_MAX = CHAR_MAX * CHAR_MAX;

    /// <summary>
    /// The maximum value of an EO three (3-byte encoded integer type)
    /// </summary>
    public const int THREE_MAX = CHAR_MAX * CHAR_MAX * CHAR_MAX;

    /// <summary>
    /// The maximum value of an EO int (4-byte encoded integer type)
    /// </summary>
    /// <remarks>
    /// NOTE: This constant stores an unsigned value of 4097152081. The <c>int</c> type is signed, meaning this value overflows.
    /// </remarks>
    public const int INT_MAX = unchecked((int)((long)SHORT_MAX * SHORT_MAX));
}
