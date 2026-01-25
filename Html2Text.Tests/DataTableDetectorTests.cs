using System;
using Html2Text.Parsing;
using NUnit.Framework;
using System.Collections.Generic;
using Html2Text.Rendering.Tables;

namespace Html2Text.Tests;

[TestFixture]
public class DataTableDetectorTests
{
    [Test]
    public void IsDataTable_Returns_False_For_TextNode()
    {
        // arrange
        var node = new Node { Chars = "some text".AsMemory() };

        // act
        var result = DataTableDetector.IsDataTable(node);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDataTable_Returns_False_For_NonTableNode()
    {
        // arrange
        var node = new Node { TagName = "div" };

        // act
        var result = DataTableDetector.IsDataTable(node);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDataTable_Returns_False_For_TableWithNoChildNodes()
    {
        // arrange
        var node = new Node { TagName = "table" };

        // act
        var result = DataTableDetector.IsDataTable(node);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDataTable_Returns_False_For_TableWithChildTextNodes()
    {
        // arrange
        var html = """
                   <table>
                       some text directly in the table
                       <thead></thead>
                       <tbody></tbody>
                   </table>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = DataTableDetector.IsDataTable(nodes[0]);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDataTable_Returns_False_For_TableWithChildNodesOtherThanTheadTbodyAndCaption()
    {
        // arrange
        var html = """
                   <table>
                       <thead></thead>
                       <p>
                         some text in a paragraph
                       </p>
                       <tbody></tbody>
                   </table>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = DataTableDetector.IsDataTable(nodes[0]);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDataTable_Returns_True_For_TableWithCaption()
    {
        // arrange
        var html = """
                   <table>
                       <caption>
                         Caption means it's likely from a real data table
                       </caption>
                   </table>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = DataTableDetector.IsDataTable(nodes[0]);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDataTable_Returns_True_For_TableWithTheadAndTbody()
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
        var result = DataTableDetector.IsDataTable(nodes[0]);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDataTable_Returns_True_For_TableWithBodyAndHeaderCells()
    {
        // arrange
        var html = """
                   <table>
                     <tbody>
                       <tr><th>Name</th><th>Age</th></tr>
                       <tr><td>Paul</td><td>34</td></tr>
                       <tr><td>Liv</td><td>26</td></tr>
                     </tbody>
                   </table>
                   """;
        List<Node> nodes = Parser.ParseHtml(html);

        // act
        var result = DataTableDetector.IsDataTable(nodes[0]);

        // assert
        Assert.That(result, Is.True);
    }
}