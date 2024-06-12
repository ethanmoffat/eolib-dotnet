using System.Reflection;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace Moffat.EndlessOnline.SDK.Protocol;

/// <summary>
/// PacketResolver resolves a packet type from a Packet ID (the PacketFamily and PacketAction).
/// </summary>
public class PacketResolver
{
    private readonly IReadOnlyDictionary<(PacketFamily Family, PacketAction Action), Type> _map;

    /// <summary>
    /// Create a new instance of a PacketResolver for the specified namespace.
    /// </summary>
    /// <param name="name_space">The fully-qualified namespace name which should be searched.</param>
    public PacketResolver(string name_space)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetTypes().Select(x => x.Namespace).Any(x => x == name_space))
            {
                _map = MapTypesFrom(assembly, name_space);
                break;
            }
        }

        if (_map == null || !_map.Any())
        {
            throw new ArgumentException($"No packets were found from {name_space}", nameof(name_space));
        }
    }

    /// <summary>
    /// Create an IPacket instance from the specified PacketFamily and PacketAction.
    /// </summary>
    /// <param name="family">The PacketFamily.</param>
    /// <param name="action">The PacketAction.</param>
    public IPacket Create(PacketFamily family, PacketAction action)
    {
        if (!_map.ContainsKey((family, action)))
        {
            throw new InvalidOperationException($"Attempted to resolve non-existent packet: {family}_{action}");
        }

        return (IPacket)Activator.CreateInstance(_map[(family, action)]);
    }

    /// <summary>
    /// Create an IPacket instance from the specified PacketFamily and PacketAction, converting it to a concrete packet type.
    /// </summary>
    /// <param name="family">The PacketFamily.</param>
    /// <param name="action">The PacketAction.</param>
    public TPacket Create<TPacket>(PacketFamily family, PacketAction action)
        where TPacket : IPacket
    {
        return (TPacket)Create(family, action);
    }

    private static IReadOnlyDictionary<(PacketFamily Family, PacketAction Action), Type> MapTypesFrom(Assembly assembly, string name_space)
    {
        var ret = new Dictionary<(PacketFamily Family, PacketAction Action), Type>();

        var types = assembly.GetTypes()
            .Where(x => x.Namespace == name_space)
            .Where(x => x.GetInterfaces().Any(x => x.Name == "IPacket"));
        foreach (var type in types)
        {
            var inst = (IPacket)Activator.CreateInstance(type);
            var fap = (inst.Family, inst.Action);
            ret.Add(fap, type);
        }

        return ret;
    }
}