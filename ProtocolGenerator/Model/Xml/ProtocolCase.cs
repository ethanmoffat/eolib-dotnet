using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolCase : ProtocolBaseInstruction
{
    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("default")]
    public bool Default { get; set; }

    [XmlElement("comment")]
    public string Comment { get; set; }

    [XmlElement("field", typeof(ProtocolFieldInstruction))]
    [XmlElement("array", typeof(ProtocolArrayInstruction))]
    [XmlElement("length", typeof(ProtocolLengthInstruction))]
    [XmlElement("dummy", typeof(ProtocolDummyInstruction))]
    [XmlElement("switch", typeof(ProtocolSwitchInstruction))]
    [XmlElement("chunked", typeof(ProtocolChunkedInstruction))]
    [XmlElement("break", typeof(ProtocolBreakInstruction))]
    public List<object> Instructions { get; set; }
}
