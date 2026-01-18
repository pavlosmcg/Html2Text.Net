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
            Is.EqualTo([
                "Opening:p @Start:0,Length:3",
                "Text:blorgfester @Start:3,Length:11",
                "Closing:p @Start:14,Length:4"
            ]));
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
            Is.EqualTo([
                "Opening:SPan @Start:0,Length:6",
                "Text:blorgfester @Start:6,Length:11",
                "Closing:spAN @Start:17,Length:7"
            ]));
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
            Is.EqualTo([
                "Opening:a @Start:0,Length:61",
                "Text:link text @Start:61,Length:9",
                "Closing:a @Start:70,Length:4"
            ]));
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
            Is.EqualTo([
                "Opening:p @Start:0,Length:22",
                "Text:paragraph text @Start:22,Length:14",
                "Closing:p @Start:36,Length:4"
            ]));
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
            Is.EqualTo([
                "Text:yadayim  @Start:0,Length:8",
                "Opening:p @Start:8,Length:15",
                "Text:higmar @Start:23,Length:6",
                "Closing:p @Start:29,Length:4"
            ]));
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
            Is.EqualTo([
                "SelfClosing:hr @Start:0,Length:5",
                "Text:framistan @Start:5,Length:9",
                "SelfClosing:br @Start:14,Length:17",
                "Text:bedoulia @Start:31,Length:8"
            ]));
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
            Is.EqualTo([
                "Text:framistan @Start:0,Length:9",
                "Opening:p @Start:9,Length:15",
                "Text:higmar @Start:24,Length:6",
                "Closing:p @Start:30,Length:4",
                "Text:bedoulia @Start:34,Length:8"
            ]));
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
            Is.EqualTo([
                "Text:minhag  @Start:0,Length:7",
                "Closing:p @Start:7,Length:4"
            ]));
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
            Is.EqualTo([
                "Text:</ForgotToClose @Start:0,Length:15"
            ]));
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
            Is.EqualTo([
                "Opening:div @Start:0,Length:5",
                "Closing:span @Start:5,Length:7"
            ]));
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
            Is.EqualTo([
                "Closing:span @Start:0,Length:7",
                "Opening:div @Start:7,Length:5"
            ]));
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
            Is.EqualTo([
                "SelfClosing:hr @Start:0,Length:4",
                "Text:framistan @Start:4,Length:9",
                "SelfClosing:br @Start:13,Length:4",
                "Text:bedoulia @Start:17,Length:8"
            ]));
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
            Is.EqualTo([
                "Text:</br> @Start:0,Length:5",
                "Opening:div @Start:5,Length:5"
            ]));
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
            Is.EqualTo([
                "SelfClosing:a @Start:0,Length:4",
                "SelfClosing:b @Start:4,Length:5"
            ]));
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
    [TestCase("<div!>")]
    [TestCase("<div@>")]
    [TestCase("<div#>")]
    [TestCase("<div.>")]
    [TestCase("<div=\"value\">")]
    [TestCase("<div\"value\">")]
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
    [TestCase("</div!>")]
    [TestCase("</div@>")]
    [TestCase("</div#>")]
    [TestCase("</div.>")]
    [TestCase("</div=\"value\">")]
    [TestCase("</div\"value\">")]
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
    [TestCase("<self!/>")]
    [TestCase("<self@/>")]
    [TestCase("<self#/>")]
    [TestCase("<self./>")]
    [TestCase("<self=\"value\"/>")]
    [TestCase("<self\"value\"/>")]
    public void GetEnumerator_Returns_TextTokens_WhenInput_IsMalformedSelfClosingTag(string badTag)
    {
        var input = $"some {badTag} text content";
        List<Chunk> result = CollectTestOutput(input);

        Assert.That(result.All(r => r.TokenType == TokenType.Text), Is.True);
        Assert.That(string.Concat(result.Select(r => r.Content)), Is.EqualTo(input));
    }

    [Test]
    public void MoveNext_Returns_False_WhenInput_ContainsOnlyScriptElement()
    {
        // arrange
        var unit = new Lexer("<script>var a = 1 / 2; b = 3 < c >= 4;</script>");

        // act
        var result = unit.MoveNext();

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsScriptThatNeverCloses()
    {
        // arrange
        var input = "before<script>unterminated";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Text:before",
            ]));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsScriptTagWithTagLikeContent()
    {
        // arrange
        var input = "text before<script>&&b&&0<b.length=5</script>text after";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Text:text before",
                "Text:text after",
            ]));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsScriptTagWithActualTagsInContent()
    {
        // arrange
        var input = "<script>parentElement.innerHTML += '<b>Appended text.</b>';</script><div>node after script</div>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Opening:div",
                "Text:node after script",
                "Closing:div"
            ]));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsUnexpectedIgnoredClosingTag()
    {
        // arrange
        var input = "<thing></script></thing>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Opening:thing",
                "Closing:thing",
            ]));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsSelfClosingIgnoredElements()
    {
        // arrange
        var input = "<one><meta/><two><meta /><three><meta><four><meta ><five><meta abc=\"123\" >";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Opening:one",
                "Opening:two",
                "Opening:three",
                "Opening:four",
                "Opening:five",
            ]));
    }

    [Test]
    [TestCase("script")]
    [TestCase("style")]
    [TestCase("template")]
    [TestCase("noscript")]
    [TestCase("canvas")]
    [TestCase("svg")]
    [TestCase("iframe")]
    [TestCase("object")]
    public void GetEnumerator_Returns_TextTokens_WhenInput_ContainsElementsToIgnore(string tagName)
    {
        // we're saying contents of ignored elements are always skipped, so tags inside script/style are not parsed as tags
        // ignored region doesn't stop until it finds the proper closing tag of the same name, case ignored

        // arrange
        var input = $"before<{tagName}>any content <p>whatsoever here</p> will be ignored</{tagName}>after";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Text:before",
                "Text:after",
            ]));
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
            DisplayTestOutput(result),
            Is.EqualTo([
                "Closing:div"
            ]));
    }

    [Test]
    public void GetEnumerator_Returns_TextTokens_WhenInput_ContainsCommentsThatNeverClose()
    {
        // arrange
        var input = "</p> stuff before <!-- comment that never closes <p>other stuff</p>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Closing:p",
                "Text: stuff before "
            ]));
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
            DisplayTestOutput(result),
            Is.EqualTo([
                "Opening:span",
                "Text:some text",
                "Text: and more after comment",
                "Closing:span"
            ]));
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
            DisplayTestOutput(result),
            Is.EqualTo([
                "Opening:span",
                "Text:<p <!-- comment inside p tag -->",
                "Text: >text inside p tag",
                "Closing:p",
                "Closing:span"
            ]));
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
            DisplayTestOutput(result),
            Is.EqualTo([
                "Opening:span",
                "Text:<p <!-- >",
                "Text:comment  -->text",
                "Closing:p",
                "Closing:span"
            ]));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_HasCommentsThatEndInsideOpeningTags()
    {
        // arrange
        var input = "<span><!-- comment <p-->>paragraph</p></span>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Opening:span",
                "Text:>paragraph",
                "Closing:p",
                "Closing:span"
            ]));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_HasCommentsThatEndInsideClosingTags()
    {
        // arrange
        var input = "<span><p><!-- comment </p-->></span>";

        // act
        List<Chunk> result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo([
                "Opening:span",
                "Opening:p",
                "Text:>",
                "Closing:span"
            ]));
    }
}
