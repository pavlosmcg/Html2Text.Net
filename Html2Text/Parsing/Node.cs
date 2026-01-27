using System;
using System.Collections.Generic;

namespace Html2Text.Parsing;

internal class Node
{
    public ReadOnlyMemory<char> TagChars { get; init; }
    public string? TagName => !TagChars.IsEmpty ? TagChars.ToString() : null;
    public ReadOnlyMemory<char> Chars { get; init; }
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
