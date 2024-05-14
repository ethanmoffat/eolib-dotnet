using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ProtocolGenerator;

public static class PostInitOutput
{
    private const string IPacketInterface = @"using Moffat.EndlessOnline.SDK.Data;

namespace Moffat.EndlessOnline.SDK.Protocol.Net;

/// <summary>
/// Object representation of a packet in the EO network protocol
/// </summary>
public interface IPacket : ISerializable
{
    /// <summary>
    /// Gets the packet family associated with this packet
    /// </summary>
    PacketFamily Family { get; }

    /// <summary>
    /// Gets the packet action associated with this packet
    /// </summary>
    PacketAction Action { get; }
}";

    private const string GlobalUsings = @"global using Moffat.EndlessOnline.SDK.Data;
global using Moffat.EndlessOnline.SDK.Protocol.Pub;";

    internal static void CreatePacketInterface(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("protocol.Net.IPacket.g", SourceText.From(IPacketInterface, Encoding.UTF8));
        context.AddSource("globalusings.g", SourceText.From(GlobalUsings, Encoding.UTF8));
    }
}
