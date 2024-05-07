using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public abstract class ProtocolBaseInstruction
{
    [XmlIgnore]
    public bool IsChunked { get; set; }
}