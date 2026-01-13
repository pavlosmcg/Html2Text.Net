using System;
using Html2Text.Parsing;
using Html2Text.Rendering;
using System.IO;
using System.Text;

namespace Html2Text;

public static class Html2Text
{
    public static string Convert(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return Convert(html.AsSpan());
    }

    public static string Convert(ReadOnlySpan<char> html)
    {
        var nodes = Parser.ParseHtml(html);
        return Renderer.GetText(nodes);
    }
}