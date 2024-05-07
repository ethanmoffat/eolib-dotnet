using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolLengthInstruction : ProtocolBaseInstruction
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("optional")]
    public string OptionalRaw { get; set; }

    [XmlIgnore]
    public bool? Optional => bool.TryParse(OptionalRaw, out var res) ? res : null;

    [XmlElement("comment")]
    public string Comment { get; set; }

    [XmlAttribute("offset")]
    public string OffsetRaw { get; set; }

    [XmlIgnore]
    public int? Offset => int.TryParse(OffsetRaw, out var res) ? res : null;
}
