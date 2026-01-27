using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Html2Text;

/// <summary>
/// Zero-allocation lookup for strings using ReadOnlySpan&lt;char&gt;.
/// Uses HashSet.GetAlternateLookup on .NET 10+, binary search on older versions.
/// </summary>
internal readonly struct ReadOnlyMemoryCharSet
{
#if NET10_0_OR_GREATER
    private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _lookup;

    public ReadOnlyMemoryCharSet(string[] values)
    {
        var set = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
        _lookup = set.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public bool Contains(ReadOnlyMemory<char> tagName)
    {
        return _lookup.Contains(tagName.Span);
    }

    public bool Contains(ReadOnlySpan<char> tagName)
    {
        return _lookup.Contains(tagName);
    }
#else
    private readonly ReadOnlyMemory<char>[] _matches;

    public ReadOnlyMemoryCharSet(string[] values)
    {
        _matches = [.. values.Select(x => x.AsMemory())];
    }

    public bool Contains(ReadOnlyMemory<char> tagName)
    {
        foreach(var match in _matches)
        {
            if (match.Span.Equals(tagName.Span, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool Contains(ReadOnlySpan<char> tagName)
    {
        foreach(var match in _matches)
        {
            if (match.Span.Equals(tagName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
#endif
}
