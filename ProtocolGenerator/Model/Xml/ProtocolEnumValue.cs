using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolEnumValue
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlText]
    public string Value { get; set; }

    [XmlElement("comment")]
    public string Comment { get; set; }
}
