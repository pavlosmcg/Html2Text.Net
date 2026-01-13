using System;

namespace Html2Text.Lexing;

internal readonly ref struct Token()
{
    public TokenType TokenType { get; init; } = TokenType.Text;

    public int StartIndex { get; init; } = 0;

    public int Length { get; init; } = 0;

    public bool HasText => Length > 0;

    public ReadOnlySpan<char> TagName { get; init; } = ReadOnlySpan<char>.Empty;
}
