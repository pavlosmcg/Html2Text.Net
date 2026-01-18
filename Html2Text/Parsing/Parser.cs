using Html2Text.Lexing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Html2Text.Parsing;

internal static class Parser
{
    public static List<Node> ParseHtml(ReadOnlySpan<char> html)
    {
        if (html.IsEmpty)
        {
            return [];
        }

        var nodeStack = new Stack<Node>();
        var results = new List<Node>();

        void AddNode(Node node)
        {
            if (nodeStack.TryPeek(out Node? parentNode))
            {
                AddToParent(parentNode, node);
            }
            else
            {
                results.Add(node);
            }
        }

        var lexer = new Lexer(html);
        
        while (lexer.MoveNext())
        {
            Token current = lexer.Current;

            switch (current.TokenType)
            {
                // text content
                case TokenType.Text:
                {
                    AddNode(new Node { Text = DecodeTextContent(current, html) });
                    break;
                }
                // start of a new node
                case TokenType.Opening:
                {
                    nodeStack.Push(new Node { TagName = current.TagName.ToString() });
                    break;
                }
                // end of a node or end of html string
                case TokenType.Closing:
                {
                    if (nodeStack.TryPeek(out Node? currentNode))
                    {
                        if (!string.Equals(currentNode.TagName, current.TagName.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            // mismatched closing tag, ignore it
                            break;
                        }
                    }

                    if (nodeStack.TryPop(out Node? nodeClosing))
                    {
                        AddNode(nodeClosing);
                    }

                    break;
                }
                // self-closing node
                case TokenType.SelfClosing:
                {
                    AddNode(new Node { TagName = current.TagName.ToString() });
                    break;
                }
            }
        }

        // if we still have nodes in the stack, but we are out of html, that means that some are unclosed, so close them now
        while (nodeStack.TryPop(out Node? nodeClosing))
        {
            AddNode(nodeClosing);
        }

        return results;
    }

    private static string? DecodeTextContent(Token token, ReadOnlySpan<char> inputHtml)
    {
        if (!token.HasText || token.StartIndex + token.Length > inputHtml.Length)
        {
            return null;
        }

        var rawText = inputHtml.Slice(token.StartIndex, token.Length).ToString();
        var decoded = WebUtility.HtmlDecode(rawText.Normalize(NormalizationForm.FormC));
        return decoded
            .Replace("\u00A0", " ") // nbsp -> space
            .Replace("\u2009", " ") // thin space -> space
            .Replace("\u200B", "")  // zero width space -> remove
            .Replace("\u00AD", ""); // soft hyphen -> remove
    }

    private static void AddToParent(Node? parentNode, Node? childNode)
    {
        if (parentNode == null || childNode == null) return;

        if (parentNode.Children == null)
        {
            parentNode.Children = [childNode];
        }
        else
        {
            parentNode.Children.Add(childNode);
        }
    }
}
