using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolFieldInstruction
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("length")]
    public string Length { get; set; }

    [XmlAttribute("padded")]
    public bool Padded { get; set; }

    [XmlAttribute("optional")]
    public bool Optional { get; set; }

    [XmlElement("comment")]
    public string Comment { get; set; }

    [XmlText]
    public string Content { get; set; }
}
