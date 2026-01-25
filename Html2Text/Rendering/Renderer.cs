using Html2Text.Parsing;
using System;
using System.Collections.Generic;
using System.Text;
using Html2Text.Rendering.Tables;
using Html2Text.Compatibility;
using System.Buffers;
using Html2Text.Extensions;

namespace Html2Text.Rendering;

internal static class Renderer
{

    public static void WriteText(List<Node>? nodes, IBufferWriter<char> documentBuilder)
    {
        if (nodes == null || nodes.Count == 0)
        {
            return;
        }

        var workStack = new Stack<RenderWorkItem>();
        var state = new RendererState();
        TableBuilder? tableBuilder = null;

        var root = new Node { TagChars = "#document".AsMemory(), Children = nodes };
        workStack.Push(new RenderWorkItem(RenderWorkType.BeforeTag, root));

        void QueueSpace()
        {
            state.PendingSpace = true;
        }

        void QueueNewLines(int count)
        {
            state.PendingNewLines = Math.Max(state.PendingNewLines, count);
        }

        void FlushPendingWhitespace()
        {
            while (state.HasPendingNewLines)
            {
                AppendNewline();
                state.PendingNewLines--;
                state.AtLineStart = true;
                state.PendingSpace = false;
            }

            if (state.PendingSpace)
            {
                if (!state.AtLineStart) AppendOutput(' ');
                state.PendingSpace = false;
            }
        }

        void AppendNewline()
        {
            if (state.RenderingTable && state.InsideTableCell)
            {
                // flatten line breaks in tables
                tableBuilder?.Append(' ');
            }
            else if (!state.AtDocumentStart)
            {
                // newlines when not at start of doc
                documentBuilder.AppendLine();
            }
        }

        void AppendOutput(char c)
        {
            if (state.RenderingTable && state.InsideTableCaption)
            {
                tableBuilder?.AppendToCaption(c);
            }
            else if (state.RenderingTable && state.InsideTableCell)
            {
                tableBuilder?.Append(c);
            }
            else
            {
                documentBuilder.Append(c);
                state.AtDocumentStart = false;
            }

            state.AtLineStart = false;
        }

        while (workStack.Count > 0)
        {
            RenderWorkItem item = workStack.Pop();
            switch (item.WorkType)
            {
                case RenderWorkType.BeforeTag:
                    HandleBeforeTag(item);
                    break;

                case RenderWorkType.AfterTag:
                    HandleAfterTag(item);
                    break;
            }
        }

        void HandleBeforeTag(RenderWorkItem item)
        {
            // any block elements required leading new lines
            if (IsBlockElement(item.Node.TagChars.Span))
            {
                QueueNewLines(1);
            }

            // check for verbatim tag start
            if (IsVerbatimElement(item.Node.TagChars.Span))
            {
                state.VerbatimTagDepth++;
            }

            // update list nesting depth
            if (IsListElement(item.Node.TagChars.Span))
            {
                state.ListNestingDepth++;
            }

            // entering a table
            if (DataTableDetector.IsDataTable(item.Node))
            {
                if (!state.RenderingTable)
                {
                    // if we are moving from document mode to table mode, emit pending
                    // whitespace before the TableBuilder starts capturing output
                    FlushPendingWhitespace();
                    tableBuilder = new TableBuilder(documentBuilder);
                }

                state.TableNestingDepth++;
            }

            // inside a real data table
            if (state.RenderingTable)
            {
                // check for caption
                if (IsTagCharsEqual(item.Node.TagChars.Span, "caption".AsSpan()))
                {
                    state.InsideTableCaption = true;
                }

                // start a new row
                if (IsTagCharsEqual(item.Node.TagChars.Span, "tr".AsSpan()))
                {
                    tableBuilder?.AppendRow();
                }
                // add a cell
                else if (IsTableCell(item.Node.TagChars.Span, out bool isHeader))
                {
                    tableBuilder?.AppendCell(isHeader);
                    state.InsideTableCell = true;
                }
            }
            else
            {
                // queue space for layout table cells
                if (IsTableCell(item.Node.TagChars.Span, out _))
                {
                    QueueSpace();
                }
            }

            // add to stack to process after children
            workStack.Push(new RenderWorkItem(RenderWorkType.AfterTag, item.Node));

            // add any children to work stack
            if (item.Node.Children != null)
            {
                for (int i = item.Node.Children.Count - 1; i >= 0; i--)
                {
                    workStack.Push(new RenderWorkItem(RenderWorkType.BeforeTag, item.Node.Children[i]));
                }
            }
        }

        void HandleAfterTag(RenderWorkItem item)
        {
            ReadOnlySpan<char> text = item.Node.Chars.Span;
            
            // special handling for hr tag
            if (IsTagCharsEqual(item.Node.TagChars.Span, "hr".AsSpan()))
            {
                QueueNewLines(2);
                text = new string('-', Constants.HorizontalRuleWidth).AsSpan();
            }

            // text node writing time
            foreach (char character in text)
            {
                if (!state.RenderingVerbatimBlock && char.IsWhiteSpace(character))
                {
                    QueueSpace();
                    continue;
                }

                FlushPendingWhitespace();

                if (state.AtLineStart && state.RenderingList)
                {
                    int listIndent = (state.ListNestingDepth - 1) * 2;
                    for (int i = 0; i < listIndent; i++)
                    {
                        AppendOutput(' ');
                    }
                    AppendOutput(' ');
                    AppendOutput('-');
                    AppendOutput(' ');
                }

                AppendOutput(character);
            }

            // check for verbatim tag finish
            if (IsVerbatimElement(item.Node.TagChars.Span))
            {
                state.VerbatimTagDepth = Math.Max(0, state.VerbatimTagDepth - 1);
            }

            // update list nesting depth
            if (IsListElement(item.Node.TagChars.Span))
            {
                state.ListNestingDepth = Math.Max(0, state.ListNestingDepth - 1);
            }

            // finish table caption
            if (state.RenderingTable && IsTagCharsEqual(item.Node.TagChars.Span, "caption".AsSpan()))
            {
                state.InsideTableCaption = false;
            }

            // finishing table cell
            if (state.RenderingTable && IsTableCell(item.Node.TagChars.Span, out _))
            {
                state.InsideTableCell = false;
            }

            // exiting a table
            if (DataTableDetector.IsDataTable(item.Node))
            {
                state.TableNestingDepth = Math.Max(0, state.TableNestingDepth - 1);

                if (!state.RenderingTable && tableBuilder != null)
                {
                    // we are now outside the table, render it with the table builder
                    tableBuilder.Build();
                    tableBuilder = null;
                }
            }

            // block element required new lines before next element
            if (IsBlockElement(item.Node.TagChars.Span))
            {
                QueueNewLines(1);
            }

            // paragraph-like elements require a newline and a clear blank line after them
            if (RequiresBlankLineAfter(item.Node.TagChars.Span, state.RenderingList))
            {
                QueueNewLines(2);
            }
        }
    }

    private static bool IsTagCharsEqual(ReadOnlySpan<char> tagChars, ReadOnlySpan<char> otherTagName)
    {
        return !tagChars.IsEmpty && tagChars.Equals(otherTagName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBlockElement(ReadOnlySpan<char> tagChars)
    {
        if (tagChars.IsEmpty) return false;
        
        // Use stack allocation for small tag names
        Span<char> buffer = stackalloc char[tagChars.Length];
        tagChars.ToLowerInvariant(buffer);
        
        // Check against known block elements
        foreach (var element in Elements.BlockElements)
        {
            if (buffer.SequenceEqual(element.AsSpan()))
                return true;
        }
        return false;
    }

    private static bool RequiresBlankLineAfter(ReadOnlySpan<char> tagChars, bool insideList)
    {
        // nested list elements do not require blank lines after them
        if (insideList && IsListElement(tagChars))
        {
            return false;
        }

        if (tagChars.IsEmpty) return false;
        
        Span<char> buffer = stackalloc char[tagChars.Length];
        tagChars.ToLowerInvariant(buffer);
        
        foreach (var element in Elements.ParagraphElements)
        {
            if (buffer.SequenceEqual(element.AsSpan()))
                return true;
        }
        return false;
    }

    private static bool IsVerbatimElement(ReadOnlySpan<char> tagChars)
    {
        if (tagChars.IsEmpty) return false;
        
        Span<char> buffer = stackalloc char[tagChars.Length];
        tagChars.ToLowerInvariant(buffer);
        
        foreach (var element in Elements.VerbatimElements)
        {
            if (buffer.SequenceEqual(element.AsSpan()))
                return true;
        }
        return false;
    }

    private static bool IsListElement(ReadOnlySpan<char> tagChars)
    {
        if (tagChars.IsEmpty) return false;
        
        Span<char> buffer = stackalloc char[tagChars.Length];
        tagChars.ToLowerInvariant(buffer);
        
        foreach (var element in Elements.ListElements)
        {
            if (buffer.SequenceEqual(element.AsSpan()))
                return true;
        }
        return false;
    }

    private static bool IsTableCell(ReadOnlySpan<char> tagChars, out bool isHeader)
    {
        isHeader = IsTagCharsEqual(tagChars, "th".AsSpan());

        if (tagChars.IsEmpty) return false;
        
        Span<char> buffer = stackalloc char[tagChars.Length];
        tagChars.ToLowerInvariant(buffer);
        
        foreach (var element in Elements.TableCellElements)
        {
            if (buffer.SequenceEqual(element.AsSpan()))
                return true;
        }
        return false;
    }
}
