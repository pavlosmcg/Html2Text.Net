using System;
using System.Collections.Generic;
using System.Linq;
using Html2Text.Parsing;

namespace Html2Text.Rendering.Tables;

internal static class DataTableDetector
{
    public static bool IsDataTable(Node node)
    {
        var tagChars = node.TagChars.Span;
        if (tagChars.IsEmpty)
        {
            return false;
        }

        if (!tagChars.Equals("table".AsSpan(), StringComparison.OrdinalIgnoreCase))
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

        var nonTextChildren = node.Children.Where(child => !child.TagChars.IsEmpty).ToList();

        // no other markup is expected in a data table
        if (nonTextChildren.Any(child => !Elements.TableChildElements.Contains(child.TagChars.Span)))
        {
            return false;
        }

        bool hasCaption = nonTextChildren.Any(c => c.TagChars.Span.Equals("caption".AsSpan(), StringComparison.OrdinalIgnoreCase));

        // if it has a caption, it's very likely to be a proper table of data
        if (hasCaption)
        {
            return true;
        }

        // head and body being present means we treat it as a data table
        var head = nonTextChildren.FirstOrDefault(c => c.TagChars.Span.Equals("thead".AsSpan(), StringComparison.OrdinalIgnoreCase));
        var body = nonTextChildren.FirstOrDefault(c => c.TagChars.Span.Equals("tbody".AsSpan(), StringComparison.OrdinalIgnoreCase));
        if (head?.TagChars.IsEmpty == false && body?.TagChars.IsEmpty == false)
        {
            return true;
        }

        // if we have a body -> tr -> th, we count it as a data table
        if (body?.Children == null)
        {
            return false;
        }

        var bodyRows = body.Children.Where(c => c.TagChars.Span.Equals("tr".AsSpan(), StringComparison.OrdinalIgnoreCase));

        foreach (var row in bodyRows)
        {
            if (row.Children != null && row.Children.Any(c => c.TagChars.Span.Equals("th".AsSpan(), StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
