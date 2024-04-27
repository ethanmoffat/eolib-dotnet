using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolLengthInstruction
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("optional")]
    public bool Optional { get; set; }

    [XmlElement("comment")]
    public string Comment { get; set; }

    [XmlAttribute("offset")]
    public int Offset { get; set; }
}
