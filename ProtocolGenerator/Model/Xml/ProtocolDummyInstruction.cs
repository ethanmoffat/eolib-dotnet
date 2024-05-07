using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolDummyInstruction : ProtocolBaseInstruction
{
    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlText]
    public string Content { get; set; }
}
