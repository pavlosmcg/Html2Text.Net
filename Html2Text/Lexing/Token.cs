using System;

namespace Html2Text.Lexing;

internal readonly ref struct Token
{
    public Token(
        TokenType tokenType,
        int startIndex,
        int length,
        ReadOnlyMemory<char> tagName)
    {
        TokenType = tokenType;
        StartIndex = startIndex;
        Length = length;
        TagName = tagName;
    }

    public TokenType TokenType { get; }
    public int StartIndex { get; }
    public int Length { get; }
    public ReadOnlyMemory<char> TagName { get; }

    public bool HasText => Length > 0;
}
