namespace ProtocolGenerator.Types;

public static class TypeConverter
{
    public static string ToPrimitive(string inputType)
    {
        if (inputType.Contains(":"))
        {
            inputType = inputType.Split(':')[0];
        }

        switch (inputType)
        {
            case "byte":
            case "char":
            case "short":
            case "three":
            case "int":
                return "int";
            case "bool":
                return "bool";
            case "blob":
                return "byte[]";
            case "string":
            case "encoded_string":
                return "string";
            default:
                return string.Empty;
        }
    }

    public static string GetType(string inputType, bool isArray = false)
    {
        if (inputType.Contains(":"))
        {
            inputType = inputType.Split(':')[0];
        }

        var ret = ToPrimitive(inputType);
        if (string.IsNullOrWhiteSpace(ret))
            ret = inputType;

        if (isArray)
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
}
