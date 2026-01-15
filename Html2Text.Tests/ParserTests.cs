using System;
using Html2Text.Parsing;
using NUnit.Framework;

namespace Html2Text.Tests;

public class ParserTests
{
    [Test]
    public void ParseHtml_Returns_Empty_WhenInput_IsEmpty()
    {
        // arrange
        string html = string.Empty;

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseHtml_Returns_NodeWithText_WhenInput_IsPlainText()
    {
        // arrange
        string html = "blorgfester";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result[0].Text, Is.EqualTo("blorgfester"));
    }

    [Test]
    public void ParseHtml_Returns_NodeWithNullTagName_WhenInput_IsPlainText()
    {
        // arrange
        string html = "blorgfester";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result[0].TagName, Is.Null);
    }

    [Test]
    public void ParseHtml_Returns_NodeWithNoChildren_WhenInput_IsPlainText()
    {
        // arrange
        string html = "blorgfester";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result[0].Children, Is.Null);
    }

    [Test]
    [TestCase("blorgfester&nbsp;framistan", "blorgfester framistan")]
    [TestCase("blorgfester &amp; framistan", "blorgfester & framistan")]
    [TestCase("blorgfe&#36;&#36;ter", "blorgfe$$ter")]
    [TestCase("Zo&euml;", "Zoë")]
    [TestCase("&#918;&#969;&#942;", "Ζωή")]
    [TestCase("1&#43;1&#61;2", "1+1=2")]
    public void ParseHtml_Returns_NodeWithTextDecoded_WhenInput_IsTextWithHtmlEscapeEntities(string html, string expectedText)
    {
        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result[0].Text, Is.EqualTo(expectedText));
    }

    [Test]
    public void ParseHtml_Returns_NodeWithTextDecoded_WhenInput_RequiresUnicodeNormalisation()
    {
        // arrange
        var name = "&#918;&#969;&#951;"; // Ζωη
        var accent = "&#769;"; // combining acute accent (tonos)
        var html = $"{name}{accent}";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result[0].Text, Is.EqualTo("Ζωη\u0301")); // Ζωή
    }

    [Test]
    public void ParseHtml_Returns_NodeWithTextDecoded_WhenInput_IsTextWithEscapedHtmlCode()
    {
        // arrange
        string html = "This &lt;em&gt;should be italic&lt;/em&gt; text.";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() => {
            Assert.That(result[0].Text, Is.EqualTo("This <em>should be italic</em> text."));
            Assert.That(result[0].Children, Is.Null);
        });
    }

    [Test]
    [TestCase("\u00A0", " ")] // nbsp -> space
    [TestCase("\u2009", " ")] // thin space -> space
    [TestCase("\u200B", "")] // zero width space -> remove
    [TestCase("\u00AD", "")] // soft hyphen -> remove
    public void ParseHtml_Returns_NodeWithSpacesReplaced_WhenInput_IsTextWithSpecialUnicodeSpaces(string input, string expectedReplacement)
    {
        // act
        var result = Parser.ParseHtml(input);

        // assert
        Assert.That(result[0].Text, Is.EqualTo(expectedReplacement));
    }

    [Test]
    public void ParseHtml_Returns_NodeWithTagName_WhenInput_IsSingleElement()
    {
        // arrange
        string html = "<p>blorgfester</p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result[0].TagName, Is.EqualTo("p"));
    }

    [Test]
    public void ParseHtml_Returns_NodeWithNoChildren_WhenInput_IsEmptyElement()
    {
        // arrange
        string html = "<div></div>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result[0].Children, Is.Null);
    }

    [Test]
    public void ParseHtml_Returns_NodeWithChildTextNode_WhenInput_IsSingleElement()
    {
        // arrange
        string html = "<p>blorgfester</p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?.Count, Is.EqualTo(1));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("blorgfester"));
            Assert.That(result[0].Children?[0].TagName, Is.Null);
            Assert.That(result[0].Children?[0].Children, Is.Null);
        });
    }

    [Test]
    public void ParseHtml_Returns_NodeWithChildren_WhenInput_HasNestedElements()
    {
        // arrange
        string html = "<p><span>blorgfester</span></p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result[0].Children?.Count, Is.EqualTo(1));
    }

    [Test]
    public void ParseHtml_Returns_NodeWithCorrectTagNames_WhenInput_HasNestedElements()
    {
        // arrange
        string html = "<p><span>blorgfester</span></p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?[0].TagName, Is.EqualTo("span"));
        });
    }

    [Test]
    public void ParseHtml_Returns_MultipleNodes_WhenInput_HasMultipleElements()
    {
        // arrange
        string html = "<p>blorgfester</p><span>framistan</span>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void ParseHtml_Returns_MultipleNodes_WhenInput_HasTextBeforeElements()
    {
        // arrange
        string html = "blorgfester<span>framistan</span>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Text, Is.EqualTo("blorgfester"));
            Assert.That(result[1].Children?[0].Text, Is.EqualTo("framistan"));
        });
    }

    [Test]
    public void ParseHtml_Returns_MultipleNodes_WhenInput_HasTextBetweenElements()
    {
        // arrange
        string html = "blorgfester<span>framistan</span>higmar<div>bedoulia</div>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result[0].Text, Is.EqualTo("blorgfester"));
            Assert.That(result[1].Children?[0].Text, Is.EqualTo("framistan"));
            Assert.That(result[2].Text, Is.EqualTo("higmar"));
            Assert.That(result[3].Children?[0].Text, Is.EqualTo("bedoulia"));
        });
    }

    [Test]
    public void ParseHtml_Returns_MultipleNodes_WhenInput_HasTextAfterElements()
    {
        // arrange
        string html = "bedoulia <p>blah</p><span>blorgfester</span>yadayim";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result[0].Text, Is.EqualTo("bedoulia "));
            Assert.That(result[1].Children?[0].Text, Is.EqualTo("blah"));
            Assert.That(result[2].Children?[0].Text, Is.EqualTo("blorgfester"));
            Assert.That(result[3].Text, Is.EqualTo("yadayim"));
        });
    }

    [Test]
    public void ParseHtml_Returns_MultipleNodes_WhenInput_HasTextAndWhitespaceBetweenElements()
    {
        // arrange
        string html = """
                      bedoulia<p>blah</p>a bit
                      of    white
                      space<span>blorgfester</span>yadayim
                      """;

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(5));
            Assert.That(result[0].Text, Is.EqualTo("bedoulia"));
            Assert.That(result[1].Children?[0].Text, Is.EqualTo("blah"));
            Assert.That(result[2].Text?.Replace("\r\n", "\n"), Is.EqualTo($"a bit\nof    white\nspace"));
            Assert.That(result[3].Children?[0].Text, Is.EqualTo("blorgfester"));
            Assert.That(result[4].Text, Is.EqualTo("yadayim"));
        });
    }

    [Test]
    public void ParseHtml_Returns_MultipleNodes_WhenInput_HasMultipleNestedElements()
    {
        // arrange
        string html = "<span><div><ul></ul><div>higmar</div></div></span><span></span>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].TagName, Is.EqualTo("span"));
            Assert.That(result[0].Children?[0].TagName, Is.EqualTo("div"));
            Assert.That(result[0].Children?[0].Children?[0].TagName, Is.EqualTo("ul"));
            Assert.That(result[0].Children?[0].Children?[1].TagName, Is.EqualTo("div"));
            Assert.That(result[0].Children?[0].Children?[1].Children?[0].Text, Is.EqualTo("higmar"));
            Assert.That(result[1].TagName, Is.EqualTo("span"));
        });
    }

    [Test]
    public void ParseHtml_Returns_NodeWithCorrectTagName_WhenInput_HasNoClosingTag()
    {
        // arrange
        string html = "<p>this never closes...";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("this never closes..."));
        });
    }

    [Test]
    public void ParseHtml_Returns_CorrectNumberOfNodes_WhenInput_HasTagsThatNeverClose()
    {
        // arrange
        string html = "<p>no closing tag<p>and another<p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("no closing tag"));
            Assert.That(result[0].Children?[1].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?[1].Children?[0].Text, Is.EqualTo("and another"));
            Assert.That(result[0].Children?[1].Children?[1].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?[1].Children?[1].Children, Is.Null);
        });
    }

    [Test]
    public void ParseHtml_Returns_NodeWithText_WhenInput_HasNoOpeningTag()
    {
        // arrange
        string html = "no opening</p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Text, Is.EqualTo("no opening"));
        });
    }

    [Test]
    public void ParseHtml_Returns_NodesWithText_WhenInput_HasSpuriousClosingTag()
    {
        // arrange
        string html = "spurious </div> closing tag";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Text, Is.EqualTo("spurious "));
            Assert.That(result[1].Text, Is.EqualTo(" closing tag"));
        });
    }

    [Test]
    public void ParseHtml_Returns_NodeWithCorrectTagName_WhenInput_HasTooManyClosingTags()
    {
        // arrange
        string html = "<p>blorgfester</p></p></p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("blorgfester"));
        });
    }

    [Test]
    public void ParseHtml_Returns_Node_WhenInput_HasMultipleClosingTags()
    {
        // arrange
        string html = "<p>before</p>after</p>end";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("before"));
            Assert.That(result[1].Text, Is.EqualTo("after"));
            Assert.That(result[2].Text, Is.EqualTo("end"));
        });
    }

    [Test]
    public void ParseHtml_Returns_NodeWithCorrectTagName_WhenInput_HasWrongClosingTag()
    {
        // arrange
        string html = "<div>wrong closing tag</p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("div"));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("wrong closing tag"));
        });
    }

    [Test]
    public void ParseHtml_Returns_NodesIgnoringSpuriousClosingTags()
    {
        // arrange
        string html = "<div>inside a div</span>still inside div</div>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("div"));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("inside a div"));
            Assert.That(result[0].Children?[1].Text, Is.EqualTo("still inside div"));
        });
    }

    [Test]
    public void ParseHtml_Returns_NodeWithNoChildren_WhenInput_TagIsSelfClosing()
    {
        // arrange
        string html = "<br>Sibling text node";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].TagName, Is.EqualTo("br"));
            Assert.That(result[0].Children, Is.Null);
            Assert.That(result[1].Text, Is.EqualTo("Sibling text node"));
        });
    }

    [Test]
    public void ParseHtml_Returns_NodeWithNoChildren_WhenInput_MultipleTagsAreSelfClosing()
    {
        // arrange
        string html = "<br>Sibling text node, not a child<br />and another<br/>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(5));
            Assert.That(result[0].TagName, Is.EqualTo("br"));
            Assert.That(result[0].Children, Is.Null);
            Assert.That(result[1].Text, Is.EqualTo("Sibling text node, not a child"));
            Assert.That(result[2].TagName, Is.EqualTo("br"));
            Assert.That(result[2].Children, Is.Null);
            Assert.That(result[3].Text, Is.EqualTo("and another"));
            Assert.That(result[4].TagName, Is.EqualTo("br"));
            Assert.That(result[4].Children, Is.Null);
        });
    }

    [Test]
    public void ParseHtml_Returns_NodesAsSiblings_WhenInput_NextTagIsSelfClosing()
    {
        // arrange
        string html = "some text<br />and a bit more";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].Text, Is.EqualTo("some text"));
            Assert.That(result[1].TagName, Is.EqualTo("br"));
            Assert.That(result[2].Text, Is.EqualTo("and a bit more"));
        });
    }

    [Test]
    public void ParseHtml_Returns_SelfClosingTagAsChildren_WhenInput_ContainsMixedTagTypes()
    {
        // arrange
        string html = "<p>first line<br>second line<br />third line</p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children, Has.Count.EqualTo(5));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("first line"));
            Assert.That(result[0].Children?[1].TagName, Is.EqualTo("br"));
            Assert.That(result[0].Children?[2].Text, Is.EqualTo("second line"));
            Assert.That(result[0].Children?[3].TagName, Is.EqualTo("br"));
            Assert.That(result[0].Children?[4].Text, Is.EqualTo("third line"));
        });
    }

    [Test]
    public void ParseHtml_Returns_Empty_WhenInput_IsIgnoredElements()
    {
        // arrange
        var html = "<script>alert('you should not see this text');</script>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseHtml_Returns_Empty_WhenInput_IsIgnoredElementsContainingOtherElements()
    {
        // arrange
        var html = "<script>const html = `<div class=\"new-content\"><p>This is a new paragraph with <strong>raw HTML</strong>.</p></div>`;</script>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseHtml_Returns_NodesWithReadableText_WhenInput_ContainsIgnoredAndIncludedElements()
    {
        // arrange
        var html = "<head><meta charset='UTF-8'/><title>Clampdown - Wikipedia</title><script>document.documentElement.className='client-js';<script></head>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("head"));
            Assert.That(result[0].Children?[0].TagName, Is.EqualTo("title"));
            Assert.That(result[0].Children?[0].Children?[0].Text, Is.EqualTo("Clampdown - Wikipedia"));
        });
    }

    [Test]
    public void ParseHtml_Returns_Nodes_IgnoringDoctype()
    {
        // arrange
        var html = "<!DOCTYPE html><head><title>Clampdown - Wikipedia</title></head>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("head"));
            Assert.That(result[0].Children?[0].TagName, Is.EqualTo("title"));
            Assert.That(result[0].Children?[0].Children?[0].Text, Is.EqualTo("Clampdown - Wikipedia"));
        });
    }

    [Test]
    public void ParseHtml_Returns_Nodes_IgnoringXmlProcessingInstructions()
    {
        // arrange
        var html = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><head><title>Clampdown - Wikipedia</title></head>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("head"));
            Assert.That(result[0].Children?[0].TagName, Is.EqualTo("title"));
            Assert.That(result[0].Children?[0].Children?[0].Text, Is.EqualTo("Clampdown - Wikipedia"));
        });
    }

    [Test]
    public void ParseHtml_Returns_Nodes_IgnoringProcessingInstructionsInBody()
    {
        // arrange
        var html = "<p>This is a paragraph.</p><?my-application data?>Some final text.";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("This is a paragraph."));
            Assert.That(result[1].Text, Is.EqualTo("Some final text."));
        });
    }

    [Test]
    public void ParseHtml_Returns_Nodes_IgnoringComments()
    {
        // arrange
        var html = "<p>This is <!--a comment inside-->a paragraph.</p>";

        // act
        var result = Parser.ParseHtml(html);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TagName, Is.EqualTo("p"));
            Assert.That(result[0].Children?.Count, Is.EqualTo(2));
            Assert.That(result[0].Children?[0].Text, Is.EqualTo("This is "));
            Assert.That(result[0].Children?[1].Text, Is.EqualTo("a paragraph."));
        });
    }
}
