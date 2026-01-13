using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Html2Text;

internal static class Elements
{
    // elements we ignore when parsing the tree of nodes
    public static readonly ImmutableHashSet<string> IgnoredElements =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "script",
            "style",
            "template",
            "meta",
            "link",
            "base",
            "noscript",
            "canvas",
            "svg",
            "iframe",
            "object",
            "embed");

    // only the following are expected as direct child elements of a true data table
    public static readonly ImmutableHashSet<string> TableChildElements =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "caption",
            "colgroup",
            "thead",
            "tbody",
            "tfoot");

    // block-level elements that should introduce line breaks when rendering
    public static readonly ImmutableHashSet<string> BlockElements =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "address",
            "article",
            "aside",
            "blockquote",
            "body",
            "canvas",
            "dd",
            "details",
            "div",
            "dl",
            "dt",
            "fieldset",
            "figcaption",
            "figure",
            "footer",
            "form",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "header",
            "hr",
            "br",
            "li",
            "main",
            "nav",
            "noscript",
            "ol",
            "p",
            "pre",
            "section",
            "table",
            "thead",
            "tbody",
            "tfoot",
            "tr",
            "caption",
            "ul",
            "legend",
            "summary",
            "audio",
            "video",
            "iframe",
            "embed",
            "object");

    // elements that should be given a clear line underneath when rendering
    public static readonly ImmutableHashSet<string> ParagraphElements =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "address",
            "article",
            "aside",
            "blockquote",
            "details",
            "fieldset",
            "figcaption",
            "figure",
            "footer",
            "form",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "header",
            "hr",
            "main",
            "nav",
            "noscript",
            "p",
            "pre",
            "section",
            "table",
            "title",
            "ol",
            "ul",
            "dl");

    // whitespace is respected inside these elements
    public static readonly ImmutableHashSet<string> VerbatimElements =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "pre",
            "textarea");

    // elements that introduce a new list nesting level
    public static readonly ImmutableHashSet<string> ListElements =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "ul",
            "ol",
            "dl");

    // table data cells that get a | separator when rendering
    public static readonly ImmutableHashSet<string> TableDataElements =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "th",
            "td");
}
