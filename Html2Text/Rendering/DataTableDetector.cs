using Html2Text.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Html2Text.Rendering;

internal static class DataTableDetector
{
    public static bool IsDataTable(Node node)
    {
        if (string.IsNullOrWhiteSpace(node.TagName))
        {
            return false;
        }

        if (!node.TagName.Equals("table", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (node.Children == null)
        {
            return false;
        }

        // if there are any direct text children, it's unlikely to be a proper data table
        // newline and whitespace text nodes are ignored here allowing for source formatting
        if (node.Children.Any(child => !string.IsNullOrWhiteSpace(child.Text)))
        {
            return false;
        }

        var nonTextChildren = node.Children.Where(child => child.TagName != null).ToList();

        // no other markup is expected in a data table
        if (nonTextChildren.Any(child => !Elements.TableChildElements.Contains(child.TagName!)))
        {
            return false;
        }

        bool hasCaption = nonTextChildren.Any(c => c.TagName!.Equals("caption", StringComparison.OrdinalIgnoreCase));

        // if it has a caption, it's very likely to be a proper table of data
        if (hasCaption)
        {
            return true;
        }

        bool hasTableHead = nonTextChildren.Any(c => c.TagName!.Equals("thead", StringComparison.OrdinalIgnoreCase));
        bool hasTableBody = nonTextChildren.Any(c => c.TagName!.Equals("tbody", StringComparison.OrdinalIgnoreCase));

        if (hasTableHead && hasTableBody)
        {
            return true;
        }

        return false;
    }
}
