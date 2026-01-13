using Html2Text.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Html2Text.Rendering;

internal static class Renderer
{
    public static string GetText(List<Node>? nodes)
    {
        if (nodes == null || nodes.Count == 0)
        {
            return string.Empty;
        }

        var resultBuilder = new StringBuilder();
        var workStack = new Stack<RenderWorkItem>();
        workStack.Push(new RenderWorkItem(RenderWorkType.BeforeTag, new Node { Children = nodes }));

        int pendingNewLines = 0;
        bool pendingSpace = false;
        bool atLineStart = true;
        bool atDocumentStart = true;
        int verbatimTagDepth = 0;
        int listNestingDepth = 0;
        int tableNestingDepth = 0;

        void RequestSpace()
        {
            if (atDocumentStart) return;

            if (pendingNewLines == 0 && !atLineStart)
                pendingSpace = true;
        }

        void RequestNewLine(int count)
        {
            if (atDocumentStart) return;

            pendingNewLines = Math.Max(pendingNewLines, count);
        }

        void FlushPendingWhitespace()
        {
            while (pendingNewLines > 0)
            {
                resultBuilder.AppendLine();
                pendingNewLines--;
                atLineStart = true;
                pendingSpace = false;
            }

            if (pendingSpace)
            {
                resultBuilder.Append(' ');
                pendingSpace = false;
            }
        }

        while (workStack.TryPop(out RenderWorkItem? item))
        {
            if (item.WorkType == RenderWorkType.BeforeTag)
            {
                // any block elements required leading new lines
                if (IsBlockElement(item.Node.TagName))
                {
                    RequestNewLine(1);
                }

                // check for verbatim tag start
                if (IsVerbatimElement(item.Node.TagName))
                {
                    verbatimTagDepth++;
                }

                // update list nesting depth
                if (IsListElement(item.Node.TagName))
                {
                    listNestingDepth++;
                }

                // entering a table
                if (DataTableDetector.IsDataTable(item.Node))
                {
                    tableNestingDepth++;
                }

                // table column separator (only for top-level tables)
                if (tableNestingDepth == 1 &&
                    !atLineStart &&
                    pendingNewLines == 0 &&
                    IsTableItem(item.Node.TagName))
                {
                    resultBuilder.Append("\t |");
                }

                // request space before table items
                if (IsTableItem(item.Node.TagName))
                {
                    RequestSpace();
                }

                // add to stack to process after children
                workStack.Push(item with { WorkType = RenderWorkType.AfterTag });

                // add any children to work stack
                if (item.Node.Children != null)
                {
                    for (int i = item.Node.Children.Count - 1; i >= 0; i--)
                    {
                        workStack.Push(new RenderWorkItem(RenderWorkType.BeforeTag, item.Node.Children[i]));
                    }
                }
            }

            if (item.WorkType == RenderWorkType.AfterTag)
            {
                string text = item.Node.Text ?? string.Empty;

                // special handling for hr tag
                if (item.Node.TagName != null &&
                    item.Node.TagName.Equals("hr", StringComparison.OrdinalIgnoreCase))
                {
                    RequestNewLine(2);
                    text = new string('-', 17);
                }

                // only for top-level tables, render a separator line for table head
                if (tableNestingDepth == 1 &&
                    item.Node.TagName != null &&
                    item.Node.TagName.Equals("thead", StringComparison.OrdinalIgnoreCase))
                {
                    text = new string('-', 17);
                }

                // text node writing time
                foreach (char character in text)
                {
                    if (verbatimTagDepth == 0 && char.IsWhiteSpace(character))
                    {
                        RequestSpace();
                        continue;
                    }

                    FlushPendingWhitespace();

                    if (atLineStart && listNestingDepth > 0)
                    {
                        resultBuilder.Append(new string(' ', (listNestingDepth - 1) * 2));
                        resultBuilder.Append(" - ");
                    }

                    resultBuilder.Append(character);
                    atDocumentStart = false;
                    atLineStart = false;
                }

                // check for verbatim tag finish
                if (IsVerbatimElement(item.Node.TagName))
                {
                    verbatimTagDepth = Math.Max(0, verbatimTagDepth - 1);
                }

                // update list nesting depth
                if (IsListElement(item.Node.TagName))
                {
                    listNestingDepth = Math.Max(0, listNestingDepth - 1);
                }

                // exiting a table
                if (DataTableDetector.IsDataTable(item.Node))
                {
                    tableNestingDepth = Math.Max(0, tableNestingDepth - 1);
                }

                // block element required new lines before next element
                if (IsBlockElement(item.Node.TagName))
                {
                    RequestNewLine(1);
                }

                // paragraph-like elements require a newline and a clear blank line after them
                if (RequiresBlankLineAfter(item.Node.TagName, listNestingDepth))
                {
                    RequestNewLine(2);
                }
            }
        }

        return resultBuilder.ToString();
    }

    private static bool IsBlockElement(string? tagName)
    {
        return !string.IsNullOrEmpty(tagName) &&
               Elements.BlockElements.Contains(tagName);
    }

    private static bool RequiresBlankLineAfter(string? tagName, int listNestingDepth)
    {
        // nested list elements do not require blank lines after them
        if (listNestingDepth > 0 && IsListElement(tagName))
        {
            return false;
        }

        return !string.IsNullOrEmpty(tagName) &&
               Elements.ParagraphElements.Contains(tagName);
    }

    private static bool IsVerbatimElement(string? tagName)
    {
        return !string.IsNullOrEmpty(tagName) &&
               Elements.VerbatimElements.Contains(tagName);
    }

    private static bool IsListElement(string? tagName)
    {
        return !string.IsNullOrEmpty(tagName) &&
               Elements.ListElements.Contains(tagName);
    }

    private static bool IsTableItem(string? tagName)
    {
        return !string.IsNullOrEmpty(tagName) &&
               Elements.TableDataElements.Contains(tagName);
    }
}
