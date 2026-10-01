using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolBreakInstruction : ProtocolBaseInstruction
{
    [XmlElement("comment")]
    public string Comment { get; set; }
}
