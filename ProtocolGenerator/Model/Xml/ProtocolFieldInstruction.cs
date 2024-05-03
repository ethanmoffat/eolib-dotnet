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
    public string PaddedRaw { get; set; }

    [XmlIgnore]
    public bool? Padded => bool.TryParse(PaddedRaw, out var res) ? res : null;

    [XmlAttribute("optional")]
    public string OptionalRaw { get; set; }

    [XmlIgnore]
    public bool? Optional => bool.TryParse(OptionalRaw, out var res) ? res : null;

    [XmlElement("comment")]
    public string Comment { get; set; }

    [XmlText]
    public string Content { get; set; }
}
