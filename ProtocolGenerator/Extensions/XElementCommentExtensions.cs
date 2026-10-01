using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ProtocolGenerator.Extensions;

/// <summary>
/// Converts XML comments (&lt;!-- --&gt;) into &lt;comment&gt; elements so they are deserialized with the rest of the model.
/// </summary>
public static class XElementCommentExtensions
{
    private const string CommentElementName = "comment";

    /// <summary>
    /// Removes XML comments from the children of the specified element (recursively) and attaches them as &lt;comment&gt; elements.
    /// </summary>
    /// <remarks>
    /// A comment attaches to the next sibling element. A comment with no following sibling element attaches to its parent.
    /// Existing &lt;comment&gt; text is kept first, followed by trailing comments, then preceding comments.
    /// </remarks>
    public static void RewriteCommentsAsElementsInPlace(this XElement element)
    {
        var pendingComments = new List<string>();
        // Snapshot the nodes: removing a comment ends a live Nodes() enumeration early
        foreach (var node in element.Nodes().ToList())
        {
            if (node is XComment comment)
            {
                pendingComments.Add(comment.Value);
                comment.Remove();
            }
            else if (node is XElement child && child.Name != CommentElementName)
            {
                child.RewriteCommentsAsElementsInPlace();
                AppendComments(child, pendingComments);
                pendingComments.Clear();
            }
        }

        AppendComments(element, pendingComments);
    }

    private static void AppendComments(XElement element, IReadOnlyList<string> comments)
    {
        var normalized = comments.Select(Normalize).Where(x => x.Length > 0).ToList();
        if (normalized.Count == 0)
            return;

        var commentElement = element.Element(CommentElementName);
        if (commentElement == null)
        {
            element.AddFirst(new XElement(CommentElementName, string.Join("\n", normalized)));
            return;
        }

        var existing = Normalize(commentElement.Value);
        if (existing.Length > 0)
            normalized.Insert(0, existing);

        commentElement.Value = string.Join("\n", normalized);
    }

    private static string Normalize(string comment)
    {
        var lines = comment
            .Split('\n')
            .Select(x => x.Trim())
            .Where(x => x.Length > 0);
        return string.Join("\n", lines);
    }
}
