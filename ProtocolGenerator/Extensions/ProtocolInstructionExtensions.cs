using System.Collections.Generic;
using System.Linq;
using ProtocolGenerator.Model.Xml;

namespace ProtocolGenerator.Extensions;

internal static class ProtocolInstructionExtensions
{
    /// <summary>
    /// Replaces each chunked section in the XML instructions with its instructions (recursively).
    /// </summary>
    public static List<object> FlattenChunked(this IEnumerable<object> instructions)
    {
        var flattened = new List<object>();
        foreach (var inst in instructions ?? Enumerable.Empty<object>())
        {
            if (inst is ProtocolChunkedInstruction pci)
                flattened.AddRange(pci.Instructions.FlattenChunked());
            else
                flattened.Add(inst);
        }
        return flattened;
    }
}
