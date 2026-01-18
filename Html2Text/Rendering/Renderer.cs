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
        var state = new RendererState();

        var root = new Node { TagName = "#document", Children = nodes };
        workStack.Push(new RenderWorkItem(RenderWorkType.BeforeTag, root));


        void QueueSpace()
        {
            if (state.AtDocumentStart) return;

            if (!state.HasPendingNewLines && !state.AtLineStart)
            {
                state.PendingSpace = true;
            }
        }

        void QueueNewLines(int count)
        {
            if (state.AtDocumentStart) return;

            state.PendingNewLines = Math.Max(state.PendingNewLines, count);
        }

        void FlushPendingWhitespace()
        {
            while (state.HasPendingNewLines)
            {
                resultBuilder.AppendLine();
                state.PendingNewLines--;
                state.AtLineStart = true;
                state.PendingSpace = false;
            }

            if (state.PendingSpace)
            {
                resultBuilder.Append(' ');
                state.PendingSpace = false;
            }
        }

        while (workStack.TryPop(out RenderWorkItem? item))
        {
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
            if (IsBlockElement(item.Node.TagName))
            {
                QueueNewLines(1);
            }

            // check for verbatim tag start
            if (IsVerbatimElement(item.Node.TagName))
            {
                state.VerbatimTagDepth++;
            }

            // update list nesting depth
            if (IsListElement(item.Node.TagName))
            {
                state.ListNestingDepth++;
            }

            // entering a table
            if (DataTableDetector.IsDataTable(item.Node))
            {
                state.TableNestingDepth++;
            }

            // table column separator (only for top-level tables)
            if (state.InsideTable &&
                !state.AtLineStart &&
                !state.HasPendingNewLines &&
                IsTableItem(item.Node.TagName))
            {
                resultBuilder.Append("\t |");
            }

            // request space before table items
            if (IsTableItem(item.Node.TagName))
            {
                QueueSpace();
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

        void HandleAfterTag(RenderWorkItem item)
        {
            string text = item.Node.Text ?? string.Empty;
            
            // special handling for hr tag
            if (IsTag(item.Node, "hr"))
            {
                QueueNewLines(2);
                text = new string('-', Constants.HorizontalRuleWidth);
            }

            // only for top-level tables, render a separator line for table head
            if (state.InsideTable &&
            IsTag(item.Node, "thead"))
            {
                text = new string('-', 17);
            }

            // text node writing time
            foreach (char character in text)
            {
                if (!state.InsideVerbatimBlock && char.IsWhiteSpace(character))
                {
                    QueueSpace();
                    continue;
                }

                FlushPendingWhitespace();

                if (state.AtLineStart && state.InsideList)
                {
                    resultBuilder.Append(new string(' ', (state.ListNestingDepth - 1) * 2));
                    resultBuilder.Append(" - ");
                }

                resultBuilder.Append(character);
                state.AtDocumentStart = false;
                state.AtLineStart = false;
            }

            // check for verbatim tag finish
            if (IsVerbatimElement(item.Node.TagName))
            {
                state.VerbatimTagDepth = Math.Max(0, state.VerbatimTagDepth - 1);
            }

            // update list nesting depth
            if (IsListElement(item.Node.TagName))
            {
                state.ListNestingDepth = Math.Max(0, state.ListNestingDepth - 1);
            }

            // exiting a table
            if (DataTableDetector.IsDataTable(item.Node))
            {
                state.TableNestingDepth = Math.Max(0, state.TableNestingDepth - 1);
            }

            // block element required new lines before next element
            if (IsBlockElement(item.Node.TagName))
            {
                QueueNewLines(1);
            }

            // paragraph-like elements require a newline and a clear blank line after them
            if (RequiresBlankLineAfter(item.Node.TagName, state.InsideList))
            {
                QueueNewLines(2);
            }
        }

        return resultBuilder.ToString();
    }

    private static bool IsTag(Node node, string name)
    {
        return node.TagName != null &&
               node.TagName.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBlockElement(string? tagName)
    {
        return !string.IsNullOrEmpty(tagName) &&
               Elements.BlockElements.Contains(tagName);
    }

    private static bool RequiresBlankLineAfter(string? tagName, bool insideList)
    {
        // nested list elements do not require blank lines after them
        if (insideList && IsListElement(tagName))
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
