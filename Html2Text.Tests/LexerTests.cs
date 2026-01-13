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

    internal IEnumerable<string> DisplayTestOutput(List<Chunk> result)
    {
        return result.Select(r => $"{r.TokenType}:{r.Content} @Start:{r.StartIndex},Length:{r.Length}");
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
    public void MoveNext_Returns_False_WhenAllInputHasBeenConsumed()
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
    public void MoveNext_Returns_True_WithTextToken_WhenInputIsOnlyText()
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
        var result = CollectTestOutput(input);

        // assert
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Opening:p @Start:0,Length:3",
                "Text:blorgfester @Start:3,Length:11",
                "Closing:p @Start:14,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTokensHaveStrangeCasing()
    {
        // arrange
        var input = "<SPan>blorgfester</spAN>";

        // act
        var result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Opening:SPan @Start:0,Length:6",
                "Text:blorgfester @Start:6,Length:11",
                "Closing:spAN @Start:17,Length:7",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTokensHaveAttributes()
    {
        // arrange
        var input = "<a style=\"123\" attr href='https://wikimediafoundation.org/' >link text</a>";

        // act
        var result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Opening:a @Start:0,Length:61",
                "Text:link text @Start:61,Length:9",
                "Closing:a @Start:70,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTokensHaveMalformedHtmlAttributes()
    {
        // arrange
        var input = "<p style=123 another=>paragraph text</p>";

        // act
        var result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Opening:p @Start:0,Length:22",
                "Text:paragraph text @Start:22,Length:14",
                "Closing:p @Start:36,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTextTokensExistBeforeFirstTags()
    {
        // arrange
        var input = "yadayim <p class=\"foo\">higmar</p>";

        // act
        var result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Text:yadayim  @Start:0,Length:8",
                "Opening:p @Start:8,Length:15",
                "Text:higmar @Start:23,Length:6",
                "Closing:p @Start:29,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenTextTokensExistAfterFinalTags()
    {
        // arrange
        var input = "framistan<p class=\"foo\">higmar</p>bedoulia";

        // act
        var result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
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
    public void GetEnumerator_Returns_TokensSuccessfully_WhenOpeningTagsAreMissing()
    {
        // arrange
        var input = "minhag </p>";

        // act
        var result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Text:minhag  @Start:0,Length:7",
                "Closing:p @Start:7,Length:4",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TextToken_WhenInput_TagNeverCompletes()
    {
        var input = "</ForgotToClose";

        var result = CollectTestOutput(input);

        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Text:</ForgotToClose @Start:0,Length:15",
            }));

        Assert.That(result[0].StartIndex, Is.EqualTo(0));
        Assert.That(result[0].Length, Is.EqualTo(input.Length));
    }

    [Test]
    public void GetEnumerator_Returns_TokensInCorrectOrder_WhenInput_HasOpeningTagNext()
    {
        var input = "<div></span>";

        var result = CollectTestOutput(input);

        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Opening:div @Start:0,Length:5",
                "Closing:span @Start:5,Length:7",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensInCorrectOrder_WhenInput_HasClosingTagNext()
    {
        var input = "</span><div>";

        var result = CollectTestOutput(input);

        Assert.That(
            DisplayTestOutput(result),
            Is.EqualTo(new[]
            {
                "Closing:span @Start:0,Length:7",
                "Opening:div @Start:7,Length:5",
            }));
    }

    [Test]
    public void GetEnumerator_Returns_TokensSuccessfully_WhenInput_ContainsScriptTagWithTagLikeContent()
    {
        // arrange
        var input = "<script>var a = 1 / 2; b = 3 < c >= 4;</script>";

        // act
        var result = CollectTestOutput(input);

        // assert
        Assert.That(
            DisplayTestOutput(result),
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
    [TestCase("<!DOCTYPE html>", true)]
    [TestCase("<!DOCTYPE html", false)]
    [TestCase("<!DOCTYPE html <", false)]
    [TestCase("<!DOCTYPE html <>", false)]
    [TestCase("<!DOCTYPE>", false)]
    [TestCase("<!DOCTYPE >", false)]
    public void GetEnumerator_HandlesDoctypeTag(string input, bool expected)
    {
        var result = CollectTestOutput(input);

        if (expected)
        {
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
        var result = CollectTestOutput(input);

        if (expected)
        {
            Assert.That(result[0].Length, Is.EqualTo(input.Length));
        }
        else
        {
            Assert.That(result.All(r => r.TokenType == TokenType.Text), Is.True);
            Assert.That(string.Concat(result.Select(r => r.Content)), Is.EqualTo(input));
        }
    }
}