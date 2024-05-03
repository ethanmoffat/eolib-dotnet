using System.Xml;
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
    public string OptionalRaw { get; set; }

    [XmlIgnore]
    public bool? Optional => bool.TryParse(OptionalRaw, out var res) ? res : null;

    [XmlElement("comment")]
    public string Comment { get; set; }

    [XmlAttribute("delimited")]
    public string DelimitedRaw { get; set; }

    [XmlIgnore]
    public bool? Delimited => bool.TryParse(DelimitedRaw, out var res) ? res : null;

    [XmlAttribute("trailing-delimiter")]
    public string TrailingDelimiterRaw { get; set; }

    [XmlIgnore]
    public bool? TrailingDelimiter => bool.TryParse(TrailingDelimiterRaw, out var res) ? res : null;
}
