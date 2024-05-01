namespace ProtocolGenerator.Types;

public static class IdentifierConverter
{
    public static string SnakeCaseToPascalCase(string inputName)
    {
        var retName = inputName.Split('_');

        var ret = string.Empty;
        foreach (var part in retName)
        {
            ret += Capitalize(part);
        }
        return ret;
    }

    public static string Capitalize(string input)
    {
        return char.ToUpper(input[0]) + input.Substring(1);
    }
}
