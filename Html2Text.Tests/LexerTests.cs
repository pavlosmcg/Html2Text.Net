using System;
using System.Collections.Generic;
using System.Linq;
using Html2Text.Lexing;
using NUnit.Framework;

namespace Html2Text.Tests;

[TestFixture]
public class LexerTests
{
    internal record Chunk(TokenType TokenType, string Content, int StartIndex, int Length);

    internal List<Chunk> CollectTestOutput(string input)
    {
        var results = new List<Chunk>();

        foreach (Token token in new Lexer(input))
        {
            var content = token.TagName.IsEmpty
                ? input.Substring(token.StartIndex, token.Length)
                : token.TagName.ToString();
            results.Add(new Chunk(token.TokenType, content, token.StartIndex, token.Length));
        }

        return results;
    }

    internal IEnumerable<string> DisplayTestOutputWithPositions(List<Chunk> result)
    {
        return result.Select(r => $"{r.TokenType}:{r.Content} @Start:{r.StartIndex},Length:{r.Length}");
    }

    internal IEnumerable<string> DisplayTestOutput(List<Chunk> result)
    {
        return result.Select(r => $"{r.TokenType}:{r.Content}");
    }

    [Test]
    public void MoveNext_Returns_False_WhenInput_IsEmpty()
    {
        // arrange
        var unit = new Lexer(ReadOnlySpan<char>.Empty);

        // act
        var result = unit.MoveNext();

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MoveNext_Returns_True_WithCurrentTextContent_WhenInput_IsSingleCharacter()
    {
        // arrange
        var input = "z";
        var unit = new Lexer(input);

        // act
        var result = unit.MoveNext();
        var output = unit.Current;

        // assert
        Assert.That(result, Is.True);
        Assert.That(output.TokenType, Is.EqualTo(TokenType.Text));
        Assert.That(output.StartIndex, Is.EqualTo(0));
        Assert.That(output.Length, Is.EqualTo(1));
    }

    [Test]
    public void MoveNext_Returns_False_WhenInput_HasBeenConsumed()
    {
        // arrange
        var input = "hello";
        var unit = new Lexer(input);

        // act first read
        var result = unit.MoveNext();
        var output = unit.Current;

        // assert first read
        Assert.That(result, Is.True);
        Assert.That(output.TokenType, Is.EqualTo(TokenType.Text));
        var content = input.Substring(output.StartIndex, output.Length);
        Assert.That(content, Is.EqualTo("hello"));

        // act second read
        result = unit.MoveNext();

        // assert second read
        Assert.That(result, Is.False);
    }

    [Test]
    public void MoveNext_Returns_True_WithTextToken_WhenInput_IsOnlyText()
    {
        // arrange
        var input = "blorgfester";
        var unit = new Lexer(input);

        // act
        var result = unit.MoveNext();
        var output = unit.Current;

        // assert
        Assert.That(result, Is.True);
        Assert.That(output.TokenType, Is.EqualTo(TokenType.Text));
        var content = input.Substring(output.StartIndex, output.Length);
        Assert.That(content, Is.EqualTo("blorgfester"));
        Assert.That(output.StartIndex, Is.EqualTo(0));
        Assert.That(output.Length, Is.EqualTo(input.Length));
    }

    [Test]
    public void MoveNext_Returns_True_WithCurrentOpeningTagToken_WhenInput_IsOpeningTag()
    {
        // arrange
        var input = "<p>";
        var unit = new Lexer(input);

        // act
        var result = unit.MoveNext();
        var output = unit.Current;

        // assert
        Assert.That(result, Is.True);
        Assert.That(output.TokenType, Is.EqualTo(TokenType.Opening));
        Assert.That(output.TagName.ToString(), Is.EqualTo("p"));
    }

    [Test]
    public void MoveNext_Returns_True_WithCurrentClosingTag_WhenInput_IsClosingTag()
    {
        // arrange
        var input = "</p>";

        var unit = new Lexer(input);

        // act
        var result = unit.MoveNext();
        var output = unit.Current;

        // assert
        Assert.That(result, Is.True);
        Assert.That(output.TokenType, Is.EqualTo(TokenType.Closing));
        Assert.That(output.TagName.ToString(), Is.EqualTo("p"));
    }

    [Test]
    public void GetEnumerator_Returns_MultipleTokens_WhenUsedInForLoop()
    {
        // arrange
        var input = "<p>blorgfester</p>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:p @Start:0,Length:3",
                "Text:blorgfester @Start:3,Length:11",
                "Closing:p @Start:14,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTokens_HaveStrangeCasing()
    {
        // arrange
        var input = "<SPan>blorgfester</spAN>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:SPan @Start:0,Length:6",
                "Text:blorgfester @Start:6,Length:11",
                "Closing:spAN @Start:17,Length:7",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTokens_HaveAttributes()
    {
        // arrange
        var input = "<a style=\"123\" attr href='https://wikimediafoundation.org/' >link text</a>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:a @Start:0,Length:61",
                "Text:link text @Start:61,Length:9",
                "Closing:a @Start:70,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTokens_HaveMalformedHtmlAttributes()
    {
        // arrange
        var input = "<p style=123 another=>paragraph text</p>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:p @Start:0,Length:22",
                "Text:paragraph text @Start:22,Length:14",
                "Closing:p @Start:36,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTextTokens_ExistBeforeFirstTags()
    {
        // arrange
        var input = "yadayim <p class=\"foo\">higmar</p>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Text:yadayim  @Start:0,Length:8",
                "Opening:p @Start:8,Length:15",
                "Text:higmar @Start:23,Length:6",
                "Closing:p @Start:29,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsSelfClosingTags()
    {
        // arrange
        var input = "<hr/>framistan<br class=\"foo\"/>bedoulia";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "SelfClosing:hr @Start:0,Length:5",
                "Text:framistan @Start:5,Length:9",
                "SelfClosing:br @Start:14,Length:17",
                "Text:bedoulia @Start:31,Length:8",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTextTokens_ExistAfterFinalTags()
    {
        // arrange
        var input = "framistan<p class=\"foo\">higmar</p>bedoulia";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Text:framistan @Start:0,Length:9",
                "Opening:p @Start:9,Length:15",
                "Text:higmar @Start:24,Length:6",
                "Closing:p @Start:30,Length:4",
                "Text:bedoulia @Start:34,Length:8",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenOpeningTags_AreMissing()
    {
        // arrange
        var input = "minhag </p>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Text:minhag  @Start:0,Length:7",
                "Closing:p @Start:7,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TextToken_WhenInput_TagNeverCompletes()
    {
        // arrange
        var input = "</ForgotToClose";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Text:</ForgotToClose @Start:0,Length:15",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensInCorrectOrder_WhenInput_HasOpeningTagNext()
    {
        // arrange
        var input = "<div></span>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:div @Start:0,Length:5",
                "Closing:span @Start:5,Length:7",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensInCorrectOrder_WhenInput_HasClosingTagNext()
    {
        // arrange
        var input = "</span><div>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Closing:span @Start:0,Length:7",
                "Opening:div @Start:7,Length:5",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_SelfClosingTokens_WhenInput_ContainsOpeningTags_ThatShouldBeSelfClosing()
    {
        // arrange
        var input = "<hr>framistan<br>bedoulia";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "SelfClosing:hr @Start:0,Length:4",
                "Text:framistan @Start:4,Length:9",
                "SelfClosing:br @Start:13,Length:4",
                "Text:bedoulia @Start:17,Length:8",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TextTokens_WhenInput_ContainsClosingTags_ThatShouldBeSelfClosing()
    {
        // arrange
        var input = "</br><div>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Text:</br> @Start:0,Length:5",
                "Opening:div @Start:5,Length:5",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_SelfClosingTokens_WhenInput_ContainsCustomSelfClosingTags()
    {
        // arrange
        var input = "<a/><b />";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "SelfClosing:a @Start:0,Length:4",
                "SelfClosing:b @Start:4,Length:5",
            }));
    }

    // TODO tag names must be followed by whitespace '/>' or '>' to be valid
    [Test]
    [TestCase("<div!>")]
    [TestCase("<div@>")]
    [TestCase("<div#>")]
    [TestCase("<div.>")]
    [TestCase("<div=\"value\">")]
    [TestCase("<div\"value\">")]
    public void GetEnumerator_Returns_TextTokens_WhenInput_IsOpeningTag_WithInvalidCharacters(string badTag)
    {
        string input = $"content {badTag} more content";
        List<Chunk> result = CollectTestOutput(input);
        Assert.That(result.All(r => r.TokenType == TokenType.Text), Is.True);
        Assert.That(string.Concat(result.Select(r => r.Content)), Is.EqualTo(input));
    }

    [Test]
    [TestCase("< div >")]
    [TestCase("<div")]
    [TestCase("<")]
    [TestCase("<div<>")]
    [TestCase("<br")]
    [TestCase("<br/")]
    [TestCase("<")]
    [TestCase(">")]
    [TestCase("<>")]
    [TestCase("< >")]
    [TestCase("</>")]
    [TestCase("blorg < fes > ter")]
    [TestCase("<div?>")]
    [TestCase("<?div>")]
    public void GetEnumerator_Returns_TextTokens_WhenInput_IsMalformedOpeningTag(string badTag)
    {
        string input = $"content {badTag} more content";
        List<Chunk> result = CollectTestOutput(input);

       Assert.That(result.All(r => r.TokenType == TokenType.Text), Is.True);
       Assert.That(string.Concat(result.Select(r => r.Content)), Is.EqualTo(input));
    }

    [Test]
    [TestCase("</div")]
    [TestCase("<")]
    [TestCase("<>")]
    [TestCase("< >")]
    [TestCase("</div<>")]
    [TestCase("< /div>")]
    [TestCase("</ div>")]
    [TestCase("</ div >")]
    [TestCase("</div/>")]
    [TestCase("<")]
    [TestCase(">")]
    [TestCase("</")]
    [TestCase("</div?>")]
    public void GetEnumerator_Returns_TextTokens_WhenInput_IsMalformedClosingTag(string badTag)
    {
        string input = $"content{badTag}";
        List<Chunk> result = CollectTestOutput(input);

        Assert.That(result.All(r => r.TokenType == TokenType.Text), Is.True);
        Assert.That(string.Concat(result.Select(r => r.Content)), Is.EqualTo(input));
    }

    [Test]
    [TestCase("self/>")]
    [TestCase("/>")]
    [TestCase("</>")]
    [TestCase("<")]
    [TestCase(">")]
    [TestCase("</br>")]
    [TestCase("</br/>")]
    [TestCase("<br/?>")]
    [TestCase("<br?>")]
    public void GetEnumerator_Returns_TextTokens_WhenInput_IsMalformedSelfClosingTag(string badTag)
    {
        var input = $"some {badTag} text content";
        List<Chunk> result = CollectTestOutput(input);

        Assert.That(result.All(r => r.TokenType == TokenType.Text), Is.True);
        Assert.That(string.Concat(result.Select(r => r.Content)), Is.EqualTo(input));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInputContainsScriptTag_WithAngleBrackets()
    {
        // arrange
        var input = "<script>var a = 1 / 2; b = 3 < c >= 4;</script>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:script @Start:0,Length:8",
                "Text:var a = 1 / 2; b = 3  @Start:8,Length:21",
                "Text:< c > @Start:29,Length:5",
                "Text:= 4; @Start:34,Length:4",
                "Closing:script @Start:38,Length:9",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInputContainsScriptTag_WithTagLikeContent()
    {
        // arrange
        var input = "<script>&&b&&0<b.length=5</script>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Opening:script",
                "Text:&&b&&0",
                "Text:<b",
                "Text:.length=5",
                "Closing:script",
            }));
    }

    [Test]
    [TestCase("<!DOCTYPE html>", true)]
    [TestCase("<!doctype html>", true)]
    [TestCase("<!docTYPE html>", true)]
    [TestCase("<!DOCTYPE html", false)]
    [TestCase("<!DOCTYPE html <", false)]
    [TestCase("<!DOCTYPE html <>", false)]
    [TestCase("<!DOCTYPE>", false)]
    [TestCase("<!DOCTYPE >", false)]
    public void GetEnumerator_HandlesDoctypeTag(string input, bool expected)
    {
        List<Chunk> result = CollectTestOutput(input);

        if (expected)
        {
            Assert.That(result[0].TokenType, Is.EqualTo(TokenType.DocType));
            Assert.That(result[0].Length, Is.EqualTo(input.Length));
        }
        else
        {
            Assert.That(result.All(r => r.TokenType == TokenType.Text), Is.True);
            Assert.That(string.Concat(result.Select(r => r.Content)), Is.EqualTo(input));
        }
    }

    [Test]
    [TestCase("<?xml?>", true)]
    [TestCase("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>", true)]
    [TestCase("<?my-application some-data ?>", true)]
    [TestCase("<?xml / ?>", true)]
    [TestCase("<?xml > ?>", false)]
    [TestCase("<?xml < ?>", false)]
    [TestCase("<??>", false)]
    [TestCase("<?>", false)]
    [TestCase("<?xml > stuff", false)]
    [TestCase("<?xml stuff", false)]
    [TestCase("<?xml >", false)]
    [TestCase("<?xml <", false)]
    [TestCase("<?xml /", false)]
    [TestCase("<?xml ?", false)]
    [TestCase("< ?xml?>", false)]
    [TestCase("<? xml?>", false)]
    public void GetEnumerator_HandlesProcessingInstruction(string input, bool expected)
    {
        List<Chunk> result = CollectTestOutput(input);

        if (expected)
        {
            Assert.That(result[0].TokenType, Is.EqualTo(TokenType.ProcessingInstruction));
            Assert.That(result[0].Length, Is.EqualTo(input.Length));
        }
        else
        {
            Assert.That(result.All(r => r.TokenType == TokenType.Text), Is.True);
            Assert.That(string.Concat(result.Select(r => r.Content)), Is.EqualTo(input));
        }
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsComments()
    {
        // arrange
        var input = @"<!-- comment --></div>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Comment:<!-- comment --> @Start:0,Length:16",
                "Closing:div @Start:16,Length:6",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TextTokens_WhenInput_ContainsCommentsThatNeverClose()
    {
        // arrange
        var input = @"</p><!-- comment that never closes <p>other stuff</p>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Closing:p @Start:0,Length:4",
                "Text:<!-- comment that never closes <p>other stuff</p> @Start:4,Length:49",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsNodesInComments()
    {
        // arrange
        var input = "<span>some text<!--a comment </span><p> over some markup </p> --> and more after comment</span>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:span @Start:0,Length:6",
                "Text:some text @Start:6,Length:9",
                "Comment:<!--a comment </span><p> over some markup </p> --> @Start:15,Length:50",
                "Text: and more after comment @Start:65,Length:23",
                "Closing:span @Start:88,Length:7",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TextTokens_WhenInput_HasInvalidCommentsInsideTags()
    {
        // arrange
        var input = "<span><p <!-- comment inside p tag --> >text inside p tag</p></span>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:span @Start:0,Length:6",
                "Text:<p <!-- comment inside p tag --> @Start:6,Length:32",
                "Text: >text inside p tag @Start:38,Length:19",
                "Closing:p @Start:57,Length:4",
                "Closing:span @Start:61,Length:7",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TextTokens_WhenInput_HasInvalidCommentStartingInsideTagAndEndingAfterTag()
    {
        // arrange
        var input = "<span><p <!-- >comment  -->text</p></span>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:span @Start:0,Length:6",
                "Text:<p <!-- > @Start:6,Length:9",
                "Text:comment  -->text @Start:15,Length:16",
                "Closing:p @Start:31,Length:4",
                "Closing:span @Start:35,Length:7",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_CommentTokens_WhenInput_HasCommentsThatEndInsideOpeningTags()
    {
        // arrange
        var input = "<span><!-- comment <p-->>paragraph</p></span>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:span @Start:0,Length:6",
                "Comment:<!-- comment <p--> @Start:6,Length:18",
                "Text:>paragraph @Start:24,Length:10",
                "Closing:p @Start:34,Length:4",
                "Closing:span @Start:38,Length:7",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_CommentTokens_WhenInput_HasCommentsThatEndInsideClosingTags()
    {
        // arrange
        var input = "<span><p><!-- comment </p-->></span>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutputWithPositions(result),
            Is.EqualTo(new[]
            {
                "Opening:span @Start:0,Length:6",
                "Opening:p @Start:6,Length:3",
                "Comment:<!-- comment </p--> @Start:9,Length:19",
                "Text:> @Start:28,Length:1",
                "Closing:span @Start:29,Length:7",
            }));
    }
}