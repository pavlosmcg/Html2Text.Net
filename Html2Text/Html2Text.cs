using System;
using Html2Text.Parsing;
using Html2Text.Rendering;

namespace Html2Text;

public static class Html2Text
{
    public static string Convert(string html)
    {
        if (html == null)
        {
            throw new ArgumentNullException(nameof(html));
        }

        var nodes = Parser.ParseHtml(html);
        return Renderer.GetText(nodes);
    }

#if NET8_0_OR_GREATER
    public static string Convert(ReadOnlySpan<char> html)
    {
        var nodes = Parser.ParseHtml(html);
        return Renderer.GetText(nodes);
    }
#endif
}