using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

[XmlRoot("protocol")]
public sealed class ProtocolSpec
{
    [XmlElement("enum")]
    public List<ProtocolEnum> Enums { get; set; }

    [XmlElement("struct")]
    public List<ProtocolStruct> Structs { get; set; }

    [XmlElement("packet")]
    public List<ProtocolPacket> Packets { get; set; }
};
