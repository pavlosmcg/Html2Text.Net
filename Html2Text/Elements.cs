using System;
using System.Collections.Generic;

namespace Html2Text;

internal static class Elements
{
    // only the following are expected as direct child elements of a true data table
    public static readonly ReadOnlyMemoryCharSet TableChildElements = new ReadOnlyMemoryCharSet(
        ["caption", "colgroup", "thead", "tbody", "tfoot", "tr", "th"]);

    // block-level elements that should introduce line breaks when rendering
    public static readonly ReadOnlyMemoryCharSet BlockElements = new ReadOnlyMemoryCharSet(
        ["address", "article", "aside", "blockquote", "body", "canvas", "dd", "details", "div", "dl", "dt",
         "fieldset", "figcaption", "figure", "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6",
         "header", "hr", "br", "li", "main", "nav", "noscript", "ol", "p", "pre", "section", "table",
         "thead", "tbody", "tfoot", "tr", "caption", "ul", "legend", "summary", "audio", "video",
         "iframe", "embed", "object"]);

    // elements that should be given a clear line underneath when rendering
    public static readonly ReadOnlyMemoryCharSet ParagraphElements = new ReadOnlyMemoryCharSet(
        ["address", "article", "aside", "blockquote", "details", "fieldset", "figcaption", "figure",
         "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "header", "hr", "main", "nav",
         "noscript", "p", "pre", "section", "table", "title", "ol", "ul", "dl"]);

    // whitespace is respected inside these elements
    public static readonly ReadOnlyMemoryCharSet VerbatimElements = new ReadOnlyMemoryCharSet(
        ["pre", "textarea"]);

    // elements that introduce a new list nesting level
    public static readonly ReadOnlyMemoryCharSet ListElements = new ReadOnlyMemoryCharSet(
        ["ul", "ol", "dl"]);

    // table data cells that get a | separator when rendering
    public static readonly ReadOnlyMemoryCharSet TableCellElements = new ReadOnlyMemoryCharSet(
        ["th", "td"]);

    // elements that are expected to be self closing
    public static readonly ReadOnlyMemoryCharSet SelfClosingNames = new ReadOnlyMemoryCharSet(
        ["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"]);

    // elements to ignore entirely along with their content
    public static readonly ReadOnlyMemoryCharSet IgnoredElements = new ReadOnlyMemoryCharSet(
        ["script", "style", "template", "meta", "link", "base", "noscript", "canvas", "svg", "iframe", "object", "embed"]);
}
