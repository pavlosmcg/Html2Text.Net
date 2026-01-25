using System;
using System.Linq;
using Html2Text.Parsing;

namespace Html2Text.Rendering.Tables;

internal static class DataTableDetector
{
    public static bool IsDataTable(Node node)
    {
        var tagName = node.TagName;
        if (tagName == null)
        {
            return false;
        }

        if (!tagName.Equals("table", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (node.Children == null)
        {
            return false;
        }

        // if there are any direct text children, it's unlikely to be a proper data table
        // newline and whitespace text nodes are ignored here allowing for source formatting
        if (node.Children.Any(child => !child.Chars.Span.IsWhiteSpace()))
        {
            return false;
        }

        var nonTextChildren = node.Children.Where(child => child.TagName != null).ToList();

        // no other markup is expected in a data table
        if (nonTextChildren.Any(child => !Elements.TableChildElements.Contains(child.TagName!)))
        {
            return false;
        }

        bool hasCaption = nonTextChildren.Any(c => string.Equals(c.TagName, "caption", StringComparison.OrdinalIgnoreCase));

        // if it has a caption, it's very likely to be a proper table of data
        if (hasCaption)
        {
            return true;
        }

        // head and body being present means we treat it as a data table
        var head = nonTextChildren.FirstOrDefault(c => string.Equals(c.TagName, "thead", StringComparison.OrdinalIgnoreCase));
        var body = nonTextChildren.FirstOrDefault(c => string.Equals(c.TagName, "tbody", StringComparison.OrdinalIgnoreCase));
        if (head != null && body !=null)
        {
            return true;
        }

        // if we have a body -> tr -> th, we count it as a data table
        if (body?.Children == null)
        {
            return false;
        }

        var bodyRows = body.Children.Where(c => string.Equals(c.TagName, "tr", StringComparison.OrdinalIgnoreCase));

        foreach (var row in bodyRows)
        {
            if (row.Children != null && row.Children.Any(c => string.Equals(c.TagName, "th", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
