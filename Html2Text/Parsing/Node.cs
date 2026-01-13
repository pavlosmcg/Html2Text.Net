using System.Collections.Generic;

namespace Html2Text.Parsing;

internal class Node
{
    public string? TagName { get; set; }
    public string? Text { get; set; }
    public List<Node>? Children { get; set; }
}
