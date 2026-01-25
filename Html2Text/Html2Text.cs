using System;
using System.Buffers;
using System.Text;
using Html2Text.Compatibility;
using Html2Text.Parsing;
using Html2Text.Rendering;

namespace Html2Text;

public static class Html2Text
{
    public static string Convert(string html)
    {
        var builder = new StringBuilderBufferWriter();
        Convert(html, builder);
        return builder.ToString();
    }

    public static void Convert(string html, IBufferWriter<char> output)
    {
        if (html == null)
        {
            throw new ArgumentNullException(nameof(html));
        }

        var nodes = Parser.ParseHtml(html);
        Renderer.WriteText(nodes, output);
    }

#if NET8_0_OR_GREATER
    public static string Convert(ReadOnlySpan<char> html)
    {
        var nodes = Parser.ParseHtml(html);
        var writer = new ArrayBufferWriter<char>(html.Length);
        Renderer.WriteText(nodes, writer);
        return writer.WrittenSpan.ToString();
    }

    public static void Convert(ReadOnlySpan<char> html, IBufferWriter<char> output)
    {
        var nodes = Parser.ParseHtml(html);
        Renderer.WriteText(nodes, output);
    }
#endif
}