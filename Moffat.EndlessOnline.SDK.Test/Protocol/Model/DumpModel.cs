using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace Moffat.EndlessOnline.SDK.Test;

public record DumpModel(PacketFamily Family, PacketAction Action, byte[] Expected, List<DumpProperty> Properties)
{
    public override string ToString() => $"{Family}_{Action}";
}

public record DumpProperty(string Type, string Name, object Value, List<DumpProperty> Children, bool Optional);
