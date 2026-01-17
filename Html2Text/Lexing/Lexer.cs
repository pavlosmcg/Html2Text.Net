using System;

namespace Html2Text.Lexing;

internal ref struct Lexer
{
    private ReadOnlySpan<char> _html;
    private int _cursor;
    private int _textStart = 0;
    private State _state = State.OutsideTag;
    private TokenType _tokenType = TokenType.Text;

    public Lexer(ReadOnlySpan<char> html)
    {
        _html = html;
        _cursor = 0;
        Current = default;
    }

    public Token Current { get; private set; }

    public Lexer GetEnumerator() => this;

    private static bool ShouldBeSelfClosingTag(ReadOnlySpan<char> tagName)
    {
        foreach (string selfClosingName in Elements.SelfClosingNames)
        {
            if (tagName.Equals(selfClosingName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static bool IsAllowedTagNameCharacter(char character)
    {
        return char.IsLetterOrDigit(character) || character == '-' || character == ':';
    }

    private enum State
    {
        OutsideTag,
        InsideTag,
        InsideComment,
    }

    public bool MoveNext()
    {
        while (_cursor < _html.Length)
        {
            // outside any tag, cruising along in text
            if (_state == State.OutsideTag)
            {
                int tagStart = _cursor;

                if (TryEnterClosingTag()
                    || TryEnterComment()
                    || TryEnterDocType()
                    || TryEnterProcessingInstruction()
                    || TryEnterOpeningTag())
                {
                    // emit any text before token start
                    if (tagStart > _textStart)
                    {
                        bool canEmitText = TryEmitTextUpTo(tagStart);
                        _textStart = tagStart; // in case reading this tag goes wrong
                        return canEmitText;
                    }
                }
            }

            // finish comments first
            if (_state == State.InsideComment)
            {
                if (TryEndComment())
                {
                    Current = new Token
                    {
                        TokenType = TokenType.Comment,
                        StartIndex = _textStart,
                        Length = _cursor - _textStart,
                    };
                    _textStart = _cursor;
                    _state = State.OutsideTag;
                    return true;
                }
                _state = State.OutsideTag;
                continue;
            }

            // process tag
            if (_state == State.InsideTag)
            {
                // read the tag name
                ReadOnlySpan<char> tagName = GetTagName();

                // cursor is now past the name
                (bool isComplete, bool abandonTag) = TryCompleteTag(tagName);
                if (isComplete)
                {
                    Current = new Token
                    {
                        TagName = tagName,
                        TokenType = _tokenType,
                        StartIndex = _textStart,
                        Length = _cursor - _textStart,
                    };
                    _textStart = _cursor;
                    _state = State.OutsideTag;
                    return true;
                }

                if (abandonTag)
                {
                    // invalid tag, abandon and return span as text
                    bool canEmitText = TryEmitTextUpTo(_cursor);
                    _textStart = _cursor;
                    _state = State.OutsideTag;
                    return canEmitText;
                }
            }

            _cursor++;
        }

        // we have reached the end of the document if we still
        // have characters remaining, return them as text now
        if (TryEmitTextUpTo(_html.Length))
        {
            _textStart = _html.Length;
            return true;
        }
        return false;
    }

    private bool TryEmitTextUpTo(int index)
    {
        if (index <= _textStart)
        {
            return false;
        }

        Current = new Token
        {
            StartIndex = _textStart,
            Length = index - _textStart,
            TokenType = TokenType.Text
        };

        return true;
    }

    private bool TryEnterComment()
    {
        if (_html[_cursor..].StartsWith("<!--", StringComparison.Ordinal))
        {
            _tokenType = TokenType.Comment;
            _state = State.InsideComment;
            _cursor += 4; // advance past '<!--'
            return true;
        }

        return false;
    }

    private bool TryEnterDocType()
    {
        if (_html[_cursor..].StartsWith("<!DOCTYPE ", StringComparison.OrdinalIgnoreCase))
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
        if (_html[_cursor..].StartsWith("<?", StringComparison.Ordinal))
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
        if (_html[_cursor..].StartsWith("</", StringComparison.Ordinal))
        {
            _tokenType = TokenType.Closing;
            _state = State.InsideTag;
            _cursor += 2; // advance past the '</'
            return true;
        }

        return false;
    }

    private bool TryEnterOpeningTag()
    {
        if (_html[_cursor..][0] == '<')
        {
            _tokenType = TokenType.Opening;
            _state = State.InsideTag;
            _cursor += 1; // advance past '<' so that we are at the tag name
            return true;
        }

        return false;
    }

    private bool TryEndComment()
    {
        while (_cursor < _html.Length)
        {
            if (_html[_cursor..].StartsWith("-->"))
            {
                // skip comments and move past end of "-->"
                _cursor += 3;
                return true;
            }
            _cursor++;
        }

        return false;
    }

    private ReadOnlySpan<char> GetTagName()
    {
        int nameStart = _cursor;

        while (_cursor < _html.Length && IsAllowedTagNameCharacter(_html[_cursor]))
        {
            _cursor++;
        }

        return _html[nameStart.._cursor];
    }

    private (bool IsMatch, bool AbandonTag) TryCompleteTag(ReadOnlySpan<char> tagName)
    {
        // tag names must be followed by whitespace, '/>', '?>, or '>' to be valid
        char afterName = _cursor < _html.Length ? _html[_cursor] : char.MinValue;
        if (!(char.IsWhiteSpace(afterName)
            || afterName == '>'
            || afterName == '/'
            || afterName == '?'))
        {
            return (IsMatch: false, AbandonTag: true);
        }

        bool containsBadChars = false;

        while (_cursor < _html.Length && _html[_cursor] != '>')
        {
            // check that we do not have '<' character present in the tag
            if (_html[_cursor] == '<')
            {
                containsBadChars = true;
            }

            _cursor++;
        }

        if (_cursor == _html.Length)
        {
            // reached the end of the document without finding '>'
            return (IsMatch: false, AbandonTag: true);
        }

        char previous = _cursor - 1 >= 0 ? _html[_cursor - 1] : char.MinValue;
        _cursor += 1; // move past '>'

        // empty tag name or malformed
        if (containsBadChars || tagName.IsEmpty)
        {
            // abandon tag to be treated as text up to _cursor
            return (IsMatch: false, AbandonTag: true);
        }

        // check for tag that should have been self-closing
        if (ShouldBeSelfClosingTag(tagName))
        {
            if (_tokenType == TokenType.Closing) // invalid tag e.g. </br>, </img>
            {
                return (IsMatch: false, AbandonTag: true);
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
                return (IsMatch: false, AbandonTag: true);
            }
        }

        // end of processing instruction
        if (_tokenType == TokenType.ProcessingInstruction)
        {
            if (previous != '?') // processing instruction can't end with just '>', must be '?>'
            {
                return (IsMatch: false, AbandonTag: true);
            }
        }
        else // not a processing instruction
        {
            if (previous == '?') // e.g. </?> or <div?>
            {
                return (IsMatch: false, AbandonTag: true);
            }
        }

        // now we have a valid tag ending
        return (IsMatch: true, AbandonTag: false);
    }
}
