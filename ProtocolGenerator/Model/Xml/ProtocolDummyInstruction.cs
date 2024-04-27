using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolDummyInstruction
{

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlElement("comment")]
    public string Comment { get; set; }

    [XmlText]
    public string Content { get; set; }
}
