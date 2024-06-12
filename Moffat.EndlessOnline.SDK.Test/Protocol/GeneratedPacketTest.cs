using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Newtonsoft.Json.Linq;

namespace Moffat.EndlessOnline.SDK.Test.Protocol;

public abstract class GeneratedPacketTest
{
    private readonly DumpModel _model;
    private readonly PacketResolver _resolver;

    protected abstract PacketType PacketType { get; }

    protected GeneratedPacketTest(DumpModel model)
    {
        _model = model;
        _resolver = PacketType == PacketType.Client
            ? new PacketResolver("Moffat.EndlessOnline.SDK.Protocol.Net.Client")
            : new PacketResolver("Moffat.EndlessOnline.SDK.Protocol.Net.Server");
    }

    [Test]
    public void Serialize_CreatesExpectedByteStream()
    {
        // todo: un-ignore these tests once conditions for break bytes in NearbyInfo are better understood
        if ((_model.Family == PacketFamily.Range && _model.Action == PacketAction.Reply) ||
            (_model.Family == PacketFamily.Players && _model.Action == PacketAction.Agree))
        {
            Assert.Ignore($"Packet ID {_model.Family}_{_model.Action} has known Serialize issues");
            return;
        }

        // todo: un-ignore these tests once fixes are merged to eo-protocol
        if ((_model.Family == PacketFamily.Quest && _model.Action == PacketAction.Accept && _model.Properties.Any(x => x.Value != null && x.Value is long i && (int)i == (int)DialogReply.Ok)) ||
            (_model.Family == PacketFamily.Account && _model.Action == PacketAction.Reply && _model.Properties.Any(x => x.Value != null && x.Value is long i && (int)i == (int)AccountReply.Changed)) ||
            (_model.Family == PacketFamily.AdminInteract && _model.Action == PacketAction.Tell) ||
            (_model.Family == PacketFamily.Recover && _model.Action == PacketAction.Player) ||
            (_model.Family == PacketFamily.Shop && _model.Action == PacketAction.Open && PacketType == PacketType.Server) ||
            (_model.Family == PacketFamily.Door && _model.Action == PacketAction.Open && PacketType == PacketType.Server) ||
            (_model.Family == PacketFamily.Character && _model.Action == PacketAction.Remove))
        {
            Assert.Ignore($"Packet ID {_model.Family}_{_model.Action} has known protocol bugs");
            return;
        }

        IPacket packetInstance = _resolver.Create(_model.Family, _model.Action);

        ApplyProperties(_model.Properties, packetInstance);

        var writer = new EoWriter();
        packetInstance.Serialize(writer);

        Assert.That(writer.ToByteArray(), Is.EqualTo(_model.Expected));
    }

    // [Test]
    // public void Deserialize_CreatesExpectedModelObject()
    // {
    // }

    private void ApplyProperties(IReadOnlyList<DumpProperty>? properties, object targetObject)
    {
        if (properties == null)
            return;

        foreach (var property in properties)
        {
            var propertyInfo = targetObject.GetType().GetProperty(GetDotNetPropertyName(property.Name));
            if (propertyInfo == null)
            {
                throw new InvalidOperationException("Unknown property: " + property.Name);
            }

            var isArray = property.Type.StartsWith("[]");
            var propertyTypeParts = property.Type.Split("::");

            if (propertyTypeParts.Length == 1 || property.Value != null)
            {
                // primitive type object
                if (!isArray)
                {
                    if (propertyInfo.SetMethod != null && property.Value != null)
                    {
                        propertyInfo.SetValue(targetObject, ConvertPrimitive(property.Type, property.Value, property.Optional));
                    }
                }
                else
                {
                    var propertyType = property.Type.Replace("[]", string.Empty);
                    if (property.Children != null)
                    {
                        var retArray = ListFromPrimitives(propertyType, property.Children.Select(x => x.Value));
                        propertyInfo.SetValue(targetObject, retArray);
                    }
                    else if (property.Type == "[]byte" && property.Value != null)
                    {
                        var retArray = Convert.FromBase64String((string)property.Value);
                        propertyInfo.SetValue(targetObject, retArray);
                    }
                }
            }
            else if (propertyTypeParts.Length == 2)
            {
                // protocol type object
                if (!isArray)
                {
                    if (property.Value == null && !property.Optional)
                    {
                        var instance = NewInstance(property.Type);
                        ApplyProperties(property.Children, instance);
                        propertyInfo.SetValue(targetObject, instance);
                    }
                    else if (propertyInfo.SetMethod != null)
                    {
                        propertyInfo.SetValue(targetObject, ConvertPrimitive(property.Type, property.Value, property.Optional));
                    }
                }
                else
                {
                    var propertyType = property.Type.Replace("[]", string.Empty);
                    if (property.Children != null)
                    {
                        var retArray = ListFromStructs(propertyType, property.Children);
                        propertyInfo.SetValue(targetObject, retArray);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Unexpected Type for DumpProperty: " + property.Type);
            }
        }

        object? ConvertPrimitive(string propertyType, object? propertyValue, bool optional)
        {
            return propertyType switch
            {
                "int" => Convert.ToInt32(propertyValue),
                "string" => Convert.ToString(propertyValue) ?? string.Empty,
                "bool" => Convert.ToBoolean(propertyValue),
                _ => optional
                    ? NullableWithValue(propertyType, propertyValue)
                    : GetTypeFromPropertyType(propertyType) switch
                        {
                            { IsEnum: true } => Convert.ToInt32(propertyValue),
                            var lookupType => Convert.ChangeType(propertyValue, lookupType),
                        },
            };
        }

        object NewInstance(string propertyType)
        {
            var type = GetTypeFromPropertyType(propertyType);
            return Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Unable to create instance of {type.FullName}");
        }

        object? NullableWithValue(string propertyType, object? propertyValue)
        {
            if (propertyValue == null)
                return null;

            var typeParam = GetTypeFromPropertyType(propertyType);
            var nullableType = typeof(Nullable<>).MakeGenericType(typeParam);
            var constructor = nullableType.GetConstructor(new[] { typeParam }) ?? throw new InvalidOperationException($"Unable to find constructor for Nullable<T> with {typeParam.Name} parameter type");
            return typeParam switch
            {
                { IsEnum: true } => constructor.Invoke(new object[] { Convert.ToInt32(propertyValue) }),
                _ => constructor.Invoke(new object[] { propertyValue }),
            };
        }

        static Type GetTypeFromPrimitiveType(string primitiveType)
        {
            return primitiveType switch
            {
                "int" => typeof(int),
                "bool" => typeof(bool),
                "string" => typeof(string),
                _ => typeof(int)
            };
        }

        Type GetTypeFromPropertyType(string propertyType)
        {
            var propertyTypeParts = propertyType.Split("::");
            var capitalizedParts = propertyTypeParts[0].Replace("[]", string.Empty).Split("/").Select(GetDotNetPropertyName).ToArray();
            var targetNamespace = $"Moffat.EndlessOnline.SDK.{string.Join(".", capitalizedParts)}";
            return GetTypeFromNamespaceAndTypeName(targetNamespace, propertyTypeParts[1]);
        }

        Type GetTypeFromNamespaceAndTypeName(string ns, string tn)
        {
            const string AssemblyNameSuffix = ",Moffat.EndlessOnline.SDK";

            var prefix = $"{_model.Family}{_model.Action}";
            if (tn.StartsWith(prefix) && tn != prefix)
            {
                // Nested types are denoted by '+': https://stackoverflow.com/a/19055021/2562283
                tn = tn.Insert(prefix.Length, $"{PacketType}Packet+");
            }
            var fullTypeName = $"{ns}.{tn}{AssemblyNameSuffix}";

            try
            {
                return Type.GetType(fullTypeName) ?? throw new InvalidOperationException($"Type not found: {fullTypeName}");
            }
            catch (InvalidOperationException)
            {
                string baseTypeName, searchTypeName;
                if (!fullTypeName.Contains("+"))
                {
                    baseTypeName = $"{ns}.{prefix}{PacketType}Packet{AssemblyNameSuffix}";

                    var withoutSuffix = fullTypeName.Replace(AssemblyNameSuffix, string.Empty);
                    searchTypeName = withoutSuffix[(withoutSuffix.LastIndexOf(".") + 1)..];
                }
                else
                {
                    baseTypeName = fullTypeName[..fullTypeName.IndexOf("+")] + AssemblyNameSuffix;
                    searchTypeName = fullTypeName.Substring(fullTypeName.IndexOf("+")+1, fullTypeName.IndexOf(",")-fullTypeName.IndexOf("+")-1);
                }

                var baseType = Type.GetType(baseTypeName) ?? throw new InvalidOperationException($"Type not found: {baseTypeName}");
                foreach (var nestedType in baseType.GetNestedTypes().Concat(baseType.GetProperties().Select(x => x.PropertyType)))
                {
                    var nestedTypesList = nestedType.GetNestedTypes();
                    if (nestedType.IsGenericType)
                    {
                        nestedTypesList = nestedType.GetGenericArguments()[0].GetNestedTypes();
                    }

                    var match = nestedTypesList.SingleOrDefault(x => x.Name == searchTypeName);
                    if (match != null)
                        return match;
                }

                throw;
            }
        }

        object? ListFromPrimitives(string elementType, IEnumerable<object> elements)
        {
            var typeParam = GetTypeFromPrimitiveType(elementType);
            var retListType = typeof(List<>).MakeGenericType(typeParam);
            var addMethod = retListType.GetMethod("Add", new Type[] { typeParam });

            var instance = Activator.CreateInstance(retListType);
            foreach (var value in elements)
            {
                addMethod?.Invoke(instance, new[] { ConvertPrimitive(elementType, value, false) });
            }
            return instance;
        }

        object? ListFromStructs(string elementType, List<DumpProperty> elements)
        {
            var typeParam = GetTypeFromPropertyType(elementType);
            var retListType = typeof(List<>).MakeGenericType(typeParam);
            var addMethod = retListType.GetMethod("Add", new Type[] { typeParam });

            var instance = Activator.CreateInstance(retListType);
            foreach (var element in elements)
            {
                var nextElement = Activator.CreateInstance(typeParam) ?? throw new InvalidOperationException($"Unable to create instance of List<{typeParam.Name}>");
                ApplyProperties(element.Children, nextElement);
                addMethod?.Invoke(instance, new object[] { nextElement });
            }

            return instance;
        }
    }

    private static string GetDotNetPropertyName(string inputName)
    {
        if (string.IsNullOrWhiteSpace(inputName))
            return inputName;

        var retName = inputName.Split('_');

        var ret = string.Empty;
        foreach (var part in retName)
        {
            ret += char.ToUpper(part[0]) + part[1..];
        }
        return ret;
    }
}

[TestFixtureSource(typeof(GeneratedPacketSource), nameof(GeneratedPacketSource.ClientInputs))]
public class GeneratedClientPacketTest : GeneratedPacketTest
{
    protected override PacketType PacketType => PacketType.Client;

    public GeneratedClientPacketTest(string _, DumpModel model)
        : base(model) { }
}

[TestFixtureSource(typeof(GeneratedPacketSource), nameof(GeneratedPacketSource.ServerInputs))]
public class GeneratedServerPacketTest : GeneratedPacketTest
{
    protected override PacketType PacketType => PacketType.Server;

    public GeneratedServerPacketTest(string _, DumpModel model)
        : base(model) { }
}

public enum PacketType
{
    Client,
    Server
}

public class GeneratedPacketSource
{
    private const string DUMP_PATH = "eo-captured-packets";

    static GeneratedPacketSource()
    {
        var clientList = new List<(string, DumpModel)>();
        var clientPackets = Directory.GetFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, DUMP_PATH, "client"));
        foreach (var path in clientPackets)
        {
            var contents = JObject.Parse(File.ReadAllText(path)).ToObject<DumpModel>();
            clientList.Add((path, contents));
        }
        ClientInputs = clientList.Select(x => new object[] { Path.GetFileNameWithoutExtension(x.Item1), x.Item2 }).ToArray();

        var serverList = new List<(string, DumpModel)>();
        var serverPackets = Directory.GetFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, DUMP_PATH, "server"));
        foreach (var path in serverPackets)
        {
            var contents = JObject.Parse(File.ReadAllText(path)).ToObject<DumpModel>();
            serverList.Add((path, contents));
        }
        ServerInputs = serverList.Select(x => new object[] { Path.GetFileNameWithoutExtension(x.Item1), x.Item2 }).ToArray();
    }

    public static object[] ClientInputs { get; }

    public static object[] ServerInputs { get; }
}
