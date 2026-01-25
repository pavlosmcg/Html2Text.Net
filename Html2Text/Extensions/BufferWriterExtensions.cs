using System;
using System.Buffers;
using System.Text;

namespace Html2Text.Extensions;

/// <summary>
/// Extensions to make IBufferWriter&lt;char&gt; feel like a StringBuilder
/// </summary>
internal static class BufferWriterExtensions
{
    public static void Append(this IBufferWriter<char> writer, char c)
    {
        var span = writer.GetSpan(1);
        span[0] = c;
        writer.Advance(1);
    }

    public static void Append(this IBufferWriter<char> writer, ReadOnlySpan<char> s)
    {
        if (s.IsEmpty) return;
        var span = writer.GetSpan(s.Length);
        s.CopyTo(span);
        writer.Advance(s.Length);
    }

    public static void Append(this IBufferWriter<char> writer, string? s)
    {
        if (string.IsNullOrEmpty(s)) return;
        Append(writer, s.AsSpan());
    }

    public static void Append(this IBufferWriter<char> writer, StringBuilder? sb)
    {
        if (sb == null || sb.Length == 0) return;

#if NETCOREAPP || NET8_0_OR_GREATER
        foreach (var chunk in sb.GetChunks())
        {
            Append(writer, chunk.Span);
        }
#else
        Append(writer, sb.ToString().AsSpan());
#endif
    }

    public static void AppendLine(this IBufferWriter<char> writer)
    {
        Append(writer, Environment.NewLine.AsSpan());
    }

    public static void AppendRepeated(this IBufferWriter<char> writer, char c, int count)
    {
        if (count <= 0) return;
        var span = writer.GetSpan(count);
        span.Slice(0, count).Fill(c);
        writer.Advance(count);
    }
}
