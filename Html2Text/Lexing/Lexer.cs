using System;

namespace Html2Text.Lexing;

internal ref struct Lexer
{
    private ReadOnlyMemory<char> _html;
    private int _cursor;
    private int _tokenStart = 0;
    private State _state = State.OutsideTag;
    private TokenType _tokenType = TokenType.Text;

    private ReadOnlyMemory<char> RemainingHtml => _html.Slice(_cursor);

    public Lexer(ReadOnlyMemory<char> html)
    {
        _html = html;
        _cursor = 0;
        Current = default;
    }

    public Token Current { get; private set; }

    public Lexer GetEnumerator() => this;

    private enum State
    {
        OutsideTag,
        InsideTag,
    }

    public bool MoveNext()
    {
        while (true)
        {
            if (_cursor >= _html.Length)
            {
                // emit trailing text (if any)
                return TryEmitToken(TokenType.Text);
            }

            switch (_state)
            {
                case State.OutsideTag:
                {
                    // keep scanning until we see '<'
                    if (_html.Span[_cursor] != '<')
                    {
                        _cursor++;
                        continue;
                    }

                    // if we have found the start of a tag, flush any text before it
                    if (TryEmitToken(TokenType.Text)) return true;

                    // no pending text, try to consume tag now that we are at '<'
                    if (TrySkipComment()) continue;
                    if (TryEnterDocType()) continue;
                    if (TryEnterProcessingInstruction()) continue;
                    if (TryEnterClosingTag()) continue;

                    // must be an opening tag if it's not any of the above
                    EnterOpeningTag();
                    continue;
                }

                case State.InsideTag:
                {
                    ReadOnlyMemory<char> tagName = GetTagName();
                    bool isValidTag = TryCompleteTag(tagName);
                    _state = State.OutsideTag;

                    if (!isValidTag)
                    {
                        // abandon tag, treat as text
                        return TryEmitToken(TokenType.Text);
                    }

                    // If ignored and opening (not self-closing), skip content + closing tag, and emit nothing
                    if (IsIgnoredTag(tagName))
                    {
                        SkipIgnoredContent(tagName);
                        continue;
                    }

                    // Emit valid tag
                    return TryEmitToken(_tokenType, tagName);
                }
            }
        }
    }

    private bool TryEmitToken(TokenType type, ReadOnlyMemory<char> tagName = default)
    {
        if (_cursor <= _tokenStart)
            return false;

        Current = new Token(tokenType: type, tagName: tagName, startIndex: _tokenStart, length: _cursor - _tokenStart);

        _tokenStart = _cursor;
        return true;
    }

    private bool TryEnterDocType()
    {
        if (RemainingHtml.Span.StartsWith("<!DOCTYPE ".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            _tokenType = TokenType.DocType;
            _state = State.InsideTag;
            _cursor += 10; // // advance past '<!DOCTYPE '
            return true;
        }

        return false;
    }

    private bool TryEnterProcessingInstruction()
    {
        if (RemainingHtml.Span.StartsWith("<?".AsSpan(), StringComparison.Ordinal))
        {
            _tokenType = TokenType.ProcessingInstruction;
            _state = State.InsideTag;
            _cursor += 2; // advance past '<?'
            return true;
        }

        return false;
    }

    private bool TryEnterClosingTag()
    {
        if (RemainingHtml.Span.StartsWith("</".AsSpan(), StringComparison.Ordinal))
        {
            _tokenType = TokenType.Closing;
            _state = State.InsideTag;
            _cursor += 2; // advance past the '</'
            return true;
        }

        return false;
    }

    private void EnterOpeningTag()
    {
        _tokenType = TokenType.Opening;
        _state = State.InsideTag;
        _cursor += 1; // advance past '<' so that we are at the tag name
    }

    private ReadOnlyMemory<char> GetTagName()
    {
        int nameStart = _cursor;

        while (_cursor < _html.Length && IsAllowedTagNameCharacter(_html.Span[_cursor]))
        {
            _cursor++;
        }

        return _html.Slice(nameStart, _cursor - nameStart);
    }

    private bool TryCompleteTag(ReadOnlyMemory<char> tagName)
    {
        // tag names must be followed by whitespace, '/>', '?>, or '>' to be valid
        char afterName = _cursor < _html.Length ? _html.Span[_cursor] : char.MinValue;
        if (!(char.IsWhiteSpace(afterName)
            || afterName == '>'
            || afterName == '/'
            || afterName == '?'))
        {
            return false;
        }

        bool containsBadChars = false;

        while (_cursor < _html.Length && _html.Span[_cursor] != '>')
        {
            // check that we do not have '<' character present in the tag
            if (_html.Span[_cursor] == '<')
            {
                containsBadChars = true;
            }

            _cursor++;
        }

        if (_cursor == _html.Length)
        {
            // reached the end of the document without finding '>'
            return false;
        }

        char previous = _cursor - 1 >= 0 ? _html.Span[_cursor - 1] : char.MinValue;
        _cursor += 1; // move past '>'

        // empty tag name or malformed
        if (containsBadChars || tagName.IsEmpty)
        {
            // abandon tag to be treated as text up to _cursor
            return false;
        }

        // check for tag that should have been self-closing
        if (ShouldBeSelfClosingTag(tagName))
        {
            if (_tokenType == TokenType.Closing) // invalid tag e.g. </br>, </img>
            {
                return false;
            }

            _tokenType = TokenType.SelfClosing;
        }

        // end of self-closing tags
        if (previous == '/')
        {
            if (_tokenType == TokenType.Opening) // e.g. <name/> is valid self-closing, so change tag type
            {
                _tokenType = TokenType.SelfClosing;
            }
            else if (_tokenType == TokenType.Closing) // not valid e.g. </div/> or </img/>
            {
                return false;
            }
        }

        // end of processing instruction
        if (_tokenType == TokenType.ProcessingInstruction)
        {
            if (previous != '?') // processing instruction can't end with just '>', must be '?>'
            {
                return false;
            }
        }
        else // not a processing instruction
        {
            if (previous == '?') // e.g. </?> or <div?>
            {
                return false;
            }
        }

        // now we have a valid tag!
        return true;
    }

    private bool TrySkipComment()
    {
        if (!RemainingHtml.Span.StartsWith("<!--".AsSpan(), StringComparison.Ordinal))
        {
            return false;
        }
        _cursor += 4; // advance past '<!--'

        while (_cursor + 2 < _html.Span.Length)
        {
            if (_html.Span[_cursor] == '-' &&
                _html.Span[_cursor + 1] == '-' &&
                _html.Span[_cursor + 2] == '>')
            {
                // skip comments and move past end of "-->"
                _cursor += 3;
                _tokenStart = _cursor;
                return true;
            }
            _cursor++;
        }

        // EOF with no end of comment, treat as comment to EOF
        _cursor = _html.Length;
        _tokenStart = _cursor;
        return true;
    }

    private void SkipIgnoredContent(ReadOnlyMemory<char> tagName)
    {
        if (_tokenType == TokenType.Closing || _tokenType == TokenType.SelfClosing)
        {
            // closing or self-closing tag, do not emit any content
            // set start of next token to current cursor
            _tokenStart = _cursor;
            return;
        }

        if (_tokenType != TokenType.Opening) return;

        // _cursor is just past the end of the opening tag
        // move forward to find matching </tagName>
        while (_cursor + 2 + tagName.Length < _html.Span.Length)
        {
            if (_html.Span[_cursor] == '<' &&
                _html.Span[_cursor + 1] == '/' &&
                _html.Span.Slice(_cursor + 2, tagName.Length).Equals(tagName.Span, StringComparison.OrdinalIgnoreCase))
            {
                // skip past '</tagName'
                _cursor += 2 + tagName.Length;

                // allow whitespace before '>' ? (HTML allows it for some reason)
                while (_cursor < _html.Span.Length && char.IsWhiteSpace(_html.Span[_cursor]))
                {
                    _cursor++;
                }

                // found the end
                if (_cursor < _html.Span.Length && _html.Span[_cursor] == '>')
                {
                    _cursor += 1; // move past '>'
                    _tokenStart = _cursor;
                    return;
                }
            }

            // not at the closing tag yet, keep moving
            _cursor++;
        }

        // EOF without closing tag
        _cursor = _html.Length;
        _tokenStart = _cursor;
    }

    private static bool ShouldBeSelfClosingTag(ReadOnlyMemory<char> tagName)
    {
        return Elements.SelfClosingNames.Contains(tagName);
    }

    private static bool IsAllowedTagNameCharacter(char character)
    {
        return char.IsLetterOrDigit(character) || character == '-' || character == ':';
    }

    private static bool IsIgnoredTag(ReadOnlyMemory<char> tagName)
    {
        return Elements.IgnoredElements.Contains(tagName);
    }
}
