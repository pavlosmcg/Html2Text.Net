using System;
using System.Buffers;
using System.Text;

namespace Html2Text.Compatibility;

/// <summary>
/// A wrapper for StringBuilder to IBufferWriter for older frameworks that do not have 
/// ArrayBufferWriter or when we want to capture output back into a StringBuilder.
/// 
/// Targets: net462, netstandard2.0
/// Can be removed when support for these frameworks is dropped.
/// </summary>
internal class StringBuilderBufferWriter : IBufferWriter<char>
{
    private readonly StringBuilder _builder = new();
    private char[]? _buffer;

    public void Advance(int count)
    {
        if (_buffer != null)
        {
            _builder.Append(_buffer, 0, count);
        }
    }

    public Memory<char> GetMemory(int sizeHint = 0) => throw new NotSupportedException();

    public Span<char> GetSpan(int sizeHint = 0)
    {
        int size = sizeHint > 0 ? sizeHint : 256;
        if (_buffer == null || _buffer.Length < size)
        {
            _buffer = new char[size];
        }
        return _buffer;
    }

    public override string ToString() => _builder.ToString();
}
