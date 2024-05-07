using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProtocolGenerator.Model.Xml;

public sealed class ProtocolSwitchInstruction : ProtocolBaseInstruction
{
    [XmlAttribute("field")]
    public string Field { get; set; }

    [XmlElement("case")]
    public List<ProtocolCase> Cases { get; set; }
}
