namespace ProtocolGenerator.Types;

public static class TypeConverter
{
    public static string ToPrimitive(string inputType)
    {
        switch (inputType)
        {
            case "byte":
            case "char":
            case "short":
            case "three":
            case "int":
            case "bool":
                return "int";
            case "blob":
                return "byte[]";
            default:
                return string.Empty;
        }
    }

    public static string GetType(string inputType, bool isArray = false)
    {
        var ret = ToPrimitive(inputType);
        if (string.IsNullOrWhiteSpace(ret))
            ret = inputType;

        if (isArray)
            ret += "[]";

        return ret;
    }
}