using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolChunkedInstruction
{
    [XmlElement("field", typeof(ProtocolFieldInstruction))]
    [XmlElement("array", typeof(ProtocolArrayInstruction))]
    [XmlElement("length", typeof(ProtocolLengthInstruction))]
    [XmlElement("dummy", typeof(ProtocolDummyInstruction))]
    [XmlElement("switch", typeof(ProtocolSwitchInstruction))]
    [XmlElement("chunked", typeof(ProtocolChunkedInstruction))]
    [XmlElement("break", typeof(ProtocolBreakInstruction))]
    public List<object> Instructions { get; set; }
}
