using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolArrayInstruction
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("length")]
    public string Length { get; set; }

    [XmlAttribute("optional")]
    public bool Optional { get; set; }

    [XmlElement("comment")]
    public string Comment { get; set; }

    [XmlAttribute("delimited")]
    public bool Delimited { get; set; }

    [XmlAttribute("trailing-delimiter")]
    public bool TrailingDelimiter { get; set; }
}
