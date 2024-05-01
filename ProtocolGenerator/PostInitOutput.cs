using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ProtocolGenerator;

public static class PostInitOutput
{
    private const string IPacketInterface = @"using EOLib.Data;

namespace EOLib.Protocol.Net;

/// <summary>
/// Object representation of a packet in the EO network protocol
/// </summary>
public interface IPacket
{
    /// <summary>
    /// Gets the packet family associated with this packet
    /// </summary>
    PacketFamily Family { get; }

    /// <summary>
    /// Gets the packet action associated with this packet
    /// </summary>
    PacketAction Action { get; }

    /// <summary>
    /// Serializes this packet to the provided <see cref=""EOLib.Data.EoWriter""/>.
    /// </summary>
    /// <param name=""writer"">The writer that this packet will be serialized to</param>
    void Serialize(EoWriter writer);
}";

    private const string GlobalUsings = @"global using EOLib.Data;
global using EOLib.Protocol.Pub;";

    internal static void CreatePacketInterface(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("protocol.Net.IPacket.g", SourceText.From(IPacketInterface, Encoding.UTF8));
        context.AddSource("globalusings.g", SourceText.From(GlobalUsings, Encoding.UTF8));
    }
}
