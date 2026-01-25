using System;
using System.Collections.Generic;

namespace Html2Text.Parsing;

internal class Node
{
    public string? TagName { get; set; }
    public ReadOnlyMemory<char> Chars { get; set; }
    public string? Text
    {
        get
        {
            // If this is an element node (has TagName), return null when no text
            if (TagName != null) return null;
            // For text nodes, always return the string representation (even if empty)
            return Chars.ToString();
        }
    }
    public List<Node>? Children { get; set; }
}
