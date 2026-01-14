using System.Collections.Generic;
using Html2Text.Parsing;
using Html2Text.Rendering;
using NUnit.Framework;

namespace Html2Text.Tests;

public class RendererTests
{
    [Test]
    public void GetText_Returns_Empty_WhenInput_IsEmpty()
    {
        // arrange
        List<Node> nodes = [];

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetText_Returns_Empty_WhenInput_IsNull()
    {
        // arrange
        List<Node>? nodes = null;

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetText_Returns_Text_WhenInput_IsOnlyText()
    {
        // arrange
        var text = "just some text";
        List<Node> nodes = Parser.ParseHtml(text);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo(text));
    }

    [Test]
    public void GetText_Returns_Text_WhenText_IsInsideANode()
    {
        // arrange
        var html = "<span>hello</span>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public void GetText_Returns_Text_WhenInput_HasTwoNodes()
    {
        // arrange
        var html = "<span>blorg</span><span>fester</span>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("blorgfester"));
    }

    [Test]
    public void GetText_Returns_Text_WhenInput_HasElementsAndTextNodes()
    {
        // arrange
        var html = "first<span>second</span><span>third</span>fourth";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("firstsecondthirdfourth"));
    }

    [Test]
    public void GetText_Returns_Text_WhenInput_HasBadlyFormedHtml()
    {
        // arrange
        var html = "<div>forgotten closing tag...<div>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("forgotten closing tag..."));
    }

    [Test]
    public void GetText_Returns_Text_WhenInput_HasNestedNodes()
    {
        // arrange
        var html = "<div>This paragraph <i>is really interesting</i>, so you should read it!</div>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("This paragraph is really interesting, so you should read it!"));
    }

    [Test]
    public void GetText_Returns_TrimmedText_WhenInput_IsBlockElements()
    {
        // arrange
        var html = "<body><div> This should have whitespace trimmed off </div></body>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("This should have whitespace trimmed off"));
    }

    [Test]
    public void GetText_Returns_NonTrimmedText_WhenInput_IsInlineElements()
    {
        // arrange
        var html = "<span>Not <i>trimmed, </i>at all </span>over here";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("Not trimmed, at all over here"));
    }

    [Test]
    public void GetText_Returns_TrimmedText_WhenInput_ContainsBlockAndInlineElements()
    {
        // arrange
        var html = "<div> This will be trimmed at the start,<span> but this won't be.</span><div> This will be on a new line and trimmed both ends! </div>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       This will be trimmed at the start, but this won't be.
                                       This will be on a new line and trimmed both ends!
                                       """));
    }

    [Test]
    public void GetText_Returns_FormattedText_WhenInput_HasDivsRequiringNewLines()
    {
        // arrange
        var html = "<body>Text before a div.<div>This is inside a div.</div></body>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       Text before a div.
                                       This is inside a div.
                                       """));
    }

    [Test]
    public void GetText_Returns_FormattedText_WhenInput_HasParagraphElements()
    {
        // arrange
        var html = "<body><p>This is a paragraph.</p>And some final free text.</body>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       This is a paragraph.

                                       And some final free text.
                                       """));
    }

    [Test]
    public void GetText_Returns_FormattedText_WhenInput_HasParagraphElementsAndWhiteSpace()
    {
        // arrange
        var html = "<body><p>This is a paragraph.</p>   <p>And another.</p></body>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       This is a paragraph.

                                       And another.
                                       """));
    }

    [Test]
    public void GetText_Returns_FormattedText_WhenInput_HasNestedParagraphElements()
    {
        // arrange
        var html = "<body><span><p>This is a paragraph.</p></span>And some final free text.</body>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       This is a paragraph.

                                       And some final free text.
                                       """));
    }

    [Test]
    public void GetText_Returns_FormattedText_WhenInput_HasParagraphsAndDivs()
    {
        // arrange
        var html = """
                   <body>
                   Text before a div.<div>This is inside a div.</div>
                   Some stuff between them.<div>This is another div.</div><div>Next to another div.</div>
                   Some stuff between those.<p>And a paragraph here.</p>
                   <div>This is a div after a paragraph.</div>
                   <div>This is a div<p>with a paragraph inside.</p>
                   </div>And some final free text.</body>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       Text before a div.
                                       This is inside a div.
                                       Some stuff between them.
                                       This is another div.
                                       Next to another div.
                                       Some stuff between those.
                                       And a paragraph here.

                                       This is a div after a paragraph.
                                       This is a div
                                       with a paragraph inside.

                                       And some final free text.
                                       """));
    }

    [Test]
    public void GetText_Returns_FormattedText_WhenInput_ContainsLineBreakElements()
    {
        // arrange
        var html = """
                   <body>
                   <p>
                   John Doe<br>
                   123 Elm Street<br>
                   Springfield, IL 62701
                   </p></body>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       John Doe
                                       123 Elm Street
                                       Springfield, IL 62701
                                       """));
    }

    [Test]
    public void GetText_Returns_TextTrimmedAtTheStartOfEachLine_WhenInput_ContainsWhitespace()
    {
        // arrange
        var html = """
                   <div>First line</div>
                     some free text
                   <div>Last line</div>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       First line
                                       some free text
                                       Last line
                                       """));
    }

    [Test]
    public void GetText_Returns_TextTrimmedAtTheEndOfEachLine_WhenInput_ContainsWhitespace()
    {
        // arrange
        var html = "<div>First line</div><span>spaces on the end here   </span><div>Last line</div>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       First line
                                       spaces on the end here
                                       Last line
                                       """));
    }

    [Test]
    public void GetText_Returns_TextWithSpaces_WhenInput_HasNewlines()
    {
        // arrange
        var html = """
                   <div>Container Start<span> inline
                   element </span>Container End</div>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("Container Start inline element Container End"));
    }

    [Test]
    public void GetText_Returns_TextWithPreservedSpaces_WhenInput_IsInlineElementsWithWhitespaceInBetween()
    {
        // arrange
        var html = """
                   <span>First bit,</span> then a middle bit <span>and then the last part.</span>

                   <span>And a whole </span>other line over here.
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("First bit, then a middle bit and then the last part. And a whole other line over here."));
    }

    [Test]
    public void GetText_Returns_TextWithoutDoubleSpaces_WhenInput_HasMultipleSpaces()
    {
        // arrange
        var html = "framistan   bedoulia";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("framistan bedoulia"));
    }

    [Test]
    public void GetText_Returns_TextWithPreformattedWhitespace_WhenInput_HasVerbatimElements()
    {
        var capybara = """

                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⣬⠷⣶⡖⠲⡄⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                       ⠀⠀⠀⠀⠀⠀⠀⣠⠶⠋⠁⠀⠸⣿⡀⠀⡁⠈⠙⠢⠤⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                       ⠀⠀⠀⠀⠀⢠⠞⠁⠀⠀⠀⠀⠀⠉⠣⠬⢧⠀⠀⠀⠀⠈⠻⣤⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                       ⠀⠀⠀⢀⡴⠃⠀⠀⢠⣴⣿⡿⠀⠀⠀⠐⠋⠀⠀⠀⠀⠀⠀⠘⠿⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                       ⠀⢀⡴⠋⠀⠀⠀⠀⠈⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠒⠒⠓⠛⠓⠶⠶⢄⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                       ⢠⠎⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠦⣀⠀⠀⠀⠀⠀⠀⠀⠀
                       ⡞⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠸⢷⡄⠀⠀⠀⠀⠀⠀
                       ⢻⣇⣹⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⢦⡀⠀⠀⠀⠀
                       ⠀⠻⣟⠋⠀⠀⠀⠀⠀⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠻⣄⠀⠀⠀
                       ⠀⠀⠀⠉⠓⠒⠊⠉⠉⢸⡙⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠀⠀⠀⠀⠘⣆⠀⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣱⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣿⠀⠀⠀⠀⠀⢻⡄⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠟⣧⡀⠀⠀⢀⡄⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡿⠇⠀⠀⠀⠀⠀⠀⢣⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⡧⢿⡀⠚⠿⢻⡆⠀⠀⠀⠀⠀⢠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇⠀⠀⠀⠀⠀⠀⠀⠘⡆
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣿⠀⠀⠈⢹⡀⠀⠀⠀⠀⣾⡆⠀⠀⠀⠀⠀⠀⠀⠀⠾⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⡇
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠨⢷⣾⠀⠸⡷⠀⠀⠀⠘⡿⠂⠀⠀⠀⢀⡴⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⡇
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡄⠳⢼⣧⡀⠀⠀⢶⡼⠦⠀⠀⠀⡞⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠃
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡇⠀⡎⣽⠿⣦⣽⣷⠿⠒⠀⠀⠀⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⠁⣴⠃⡿⠀⠀⢠⠆⠢⡀⠀⠀⠀⠈⢧⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⠇⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣠⣠⠏⠀⣸⢰⡇⠀⢠⠏⠀⠀⠘⢦⣀⣀⠀⢀⠙⢧⡀⠀⠀⠀⠀⠀⠀⠀⠀⡰⠁⠀⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠾⠿⢯⣤⣆⣤⣯⠼⠀⠀⢸⠀⠀⠀⠀⠀⣉⠭⠿⠛⠛⠚⠟⡇⠀⠀⣀⠀⢀⡤⠊⠀⠀⠀⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠀⢸⣷⣶⣤⣦⡼⠀⠀⠀⣴⣯⠇⡀⣀⣀⠤⠤⠖⠁⠐⠚⠛⠉⠁⠀⠀⠀⠀⠀⠀
                       ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣛⠁⢋⡀⠀⠀⠀⠀⣛⣛⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀

                       """;
        // arrange
        var html = $"<pre>{capybara}</pre>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo(capybara));
    }

    [Test]
    public void GetText_Returns_TextWithPreformattedWhitespace_WhenInput_HasVerbatimElementsContainingOtherElements()
    {
        // arrange
        var html = """
                   <pre><code>let i = 5;

                   if (i < 10 && i > 0)
                     return "Single Digit Number"</code></pre>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        Assert.That(result, Is.EqualTo("""
                                       let i = 5;

                                       if (i < 10 && i > 0)
                                         return "Single Digit Number"
                                       """));
    }

    [Test]
    public void GetText_Returns_TextWithPreformattedWhitespace_WhenInput_HasVerbatimElementsContainingOtherBlockElements()
    {
        // arrange
        var html = """
                   <pre><code><p>let i = 5;</p>
                   if (i < 10 && i > 0)
                     return "Single Digit Number"</code></pre>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        var expected = """
                       let i = 5;


                       if (i < 10 && i > 0)
                         return "Single Digit Number"
                       """;
        Assert.That(result.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
    }

    [Test]
    public void GetText_Returns_TextWithPreformattedWhitespace_WhenInput_HasVerbatimElementsContainingOtherInlineElements()
    {
        // arrange
        var html = """
                   <pre class="lang-cs s-code-block"><code data-highlighted="yes" class="hljs language-csharp"><span class="hljs-function"><span class="hljs-keyword">private</span> <span class="hljs-built_in">int</span> <span class="hljs-title">GetSmallestNonNegative</span>(<span class="hljs-params"><span class="hljs-built_in">int</span> a , <span class="hljs-built_in">int</span> b</span>)</span>
                   {
                       <span class="hljs-keyword">if</span> (a &gt;= <span class="hljs-number">0</span> &amp;&amp; b &gt;= <span class="hljs-number">0</span>)
                           <span class="hljs-keyword">return</span> Math.Min(a,b);
                       <span class="hljs-keyword">else</span> <span class="hljs-keyword">if</span> (a &gt;= <span class="hljs-number">0</span> &amp;&amp; b &lt; <span class="hljs-number">0</span>)
                           <span class="hljs-keyword">return</span> a;
                       <span class="hljs-keyword">else</span> <span class="hljs-keyword">if</span> (a &lt; <span class="hljs-number">0</span> &amp;&amp; b &gt;= <span class="hljs-number">0</span>)
                           <span class="hljs-keyword">return</span> b;
                       <span class="hljs-keyword">else</span>
                           <span class="hljs-keyword">return</span> <span class="hljs-number">0</span>;
                   }
                   </code></pre>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        var expected = """
                       private int GetSmallestNonNegative(int a , int b)
                       {
                           if (a >= 0 && b >= 0)
                               return Math.Min(a,b);
                           else if (a >= 0 && b < 0)
                               return a;
                           else if (a < 0 && b >= 0)
                               return b;
                           else
                               return 0;
                       }
                       
                       """;
        Assert.That(result.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
    }

    [Test]
    public void GetText_Returns_TextWithPreformattedWhitespace_WhenInput_HasMultiNestedVerbatimElements()
    {
        // arrange
        var html = """
                   <pre><pre>
                   some
                     <span>preformatted</span>
                   </pre>text</pre>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        var expected = """

                       some
                         preformatted


                       text
                       """;
        Assert.That(result.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
    }

    [Test]
    public void GetText_Returns_TextWithAddedHorizontalLines_WhenInput_ContainsHr()
    {
        // arrange
        var html ="<div>very interesting</div><hr/><div>text here</div>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        var expected = """
                       very interesting

                       -----------------

                       text here
                       """;

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetText_Returns_TextWithIndentedLists_WhenInput_HasUnorderedList()
    {
        // arrange
        var html = """
                   <ul>
                     <li>Coffee</li>
                     <li>Tea
                       <ul>
                         <li>With lemon</li>
                         <li>With milk</li>
                       </ul>
                     </li>
                     <li>Juice</li>
                   </ul>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        var expected = """
                        - Coffee
                        - Tea
                          - With lemon
                          - With milk
                        - Juice
                       """;

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetText_Returns_TextWithTableFormatting_WhenInput_HasSimpleTable()
    {
        // arrange
        var html = """
                   <table>
                     <thead>
                       <tr><th>Name</th><th>Age</th></tr>
                     </thead>
                     <tbody>
                       <tr><td>Paul</td><td>34</td></tr>
                       <tr><td>Liv</td><td>26</td></tr>
                     </tbody>
                   </table>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        var expected = """
                       Name	 | Age
                       -----------------
                       Paul	 | 34
                       Liv	 | 26
                       """;

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetText_Returns_TextWithoutTableFormatting_WhenInput_AttemptsToUseTableForLayout()
    {
        // arrange
        var html = """
                   <table>
                     <tr>
                       <td><h2>Title</h2></td>
                       <td><p>Email message text</p></td>
                     </tr>
                   </table>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        var expected = """
                   Title
                   
                   Email message text
                   """;

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetText_Returns_TextWithSpacing_WhenInput_AttemptsToUseTableForLayout()
    {
        // arrange
        var html = "<table><tr><th scope=\"row\">Artist</th><td>The Clash</td></tr><tr><th scope=\"row\">Released</th><td>1980</td></tr></table>";
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = Renderer.GetText(nodes);

        // assert
        var expected = """
                       Artist The Clash
                       Released 1980
                       """;

        Assert.That(result, Is.EqualTo(expected));
    }
}
