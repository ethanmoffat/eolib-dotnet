using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolEnum
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlElement("value")]
    public List<ProtocolEnumValue> Values { get; set; }

    [XmlElement("comment")]
    public string Comment { get; set; }
}
