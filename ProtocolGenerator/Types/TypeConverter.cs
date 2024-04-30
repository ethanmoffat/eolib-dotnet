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
}