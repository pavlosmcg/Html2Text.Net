using Html2Text.Lexing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Html2Text.Parsing;

internal static class Parser
{
    public static List<Node> ParseHtml(string html) => ParseHtml(html.AsSpan());

    public static List<Node> ParseHtml(ReadOnlySpan<char> html)
    {
        if (html.IsEmpty)
        {
            return [];
        }

        var nodeStack = new Stack<Node>();
        var results = new List<Node>();

        var lexer = new Lexer(html);
        
        while (lexer.MoveNext())
        {
            Token current = lexer.Current;

            switch (current.TokenType)
            {
                // text content
                case TokenType.Text:
                {
                    AddNode(results, nodeStack, new Node { Text = DecodeTextContent(current, html) });
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
                    if (nodeStack.Count > 0)
                    {
                        Node currentNode = nodeStack.Peek();
                        if (currentNode != null && !current.TagName.Equals(currentNode.TagName.AsSpan(),
                                StringComparison.OrdinalIgnoreCase))
                        {
                            // mismatched closing tag, ignore it
                            break;
                        }

                        Node nodeClosing = nodeStack.Pop();
                        AddNode(results, nodeStack, nodeClosing);
                    }

                    break;
                }
                // self-closing node
                case TokenType.SelfClosing:
                {
                    AddNode(results, nodeStack, new Node { TagName = current.TagName.ToString() });
                    break;
                }
            }
        }

        // if we still have nodes in the stack, but we are out of html, that means that some are unclosed, so close them now
        while (nodeStack.Count > 0)
        {
            Node nodeClosing = nodeStack.Pop();
            AddNode(results, nodeStack, nodeClosing);
        }

        return results;
    }

    private static string? DecodeTextContent(Token token, ReadOnlySpan<char> inputHtml)
    {
        if (!token.HasText || token.StartIndex + token.Length > inputHtml.Length)
        {
            return null;
        }

        var text = inputHtml.Slice(token.StartIndex, token.Length);

        // only do html escape character decoding and funky unicode space replacing if necessary
        // a singe pass here just to check is cheaper than always doing the decoding for every tag name
        var needsDecoding = false;
        var needsUnicodeSpacesReplacing = false;
        foreach (char character in text)
        {
            if (character == '&')
            {
                needsDecoding = true;
            }

            if (character == '\u00A0' ||    // nbsp
                character == '\u2009' ||    // thin space
                character == '\u200B' ||    // zero width space
                character == '\u00AD')      // soft hyphen
            {
                needsUnicodeSpacesReplacing = true;
            }

            if (needsDecoding && needsUnicodeSpacesReplacing)
            {
                break;
            }
        }

        // if we don't need it, don't do it. Only a single string allocation here:
        var result = text.ToString();
        if (!needsDecoding && !needsUnicodeSpacesReplacing)
        {
            return result;
        }

        if (needsDecoding)
        {
            result = WebUtility.HtmlDecode(result.Normalize(NormalizationForm.FormC));
        }

        // do replacement after decoding, which can apparently cause these spaces to appear
        // relevant unit test: "blorgfester&nbsp;framistan"
        // so basically we may have had more of them appear in addition to those detected earlier
        if (needsDecoding || needsUnicodeSpacesReplacing)
        {
            char[] buffer = new char[result.Length];

            int i = 0;
            for (int j = 0; j < result.Length; j++)
            {
                if (result[j] == '\u00A0' ||        // nbsp -> space
                    result[j] == '\u2009')          // thin space -> space
                {
                    buffer[i] = ' ';
                    i++;
                }
                else if (result[j] == '\u200B' ||   // zero width space -> remove
                         result[j] == '\u00AD')     // soft hyphen -> remove
                {
                    // skip
                }
                else
                {
                    // add all normal characters
                    buffer[i] = result[j];
                    i++;
                }
            }

            result = new string(buffer, startIndex: 0, length: i);
        }

        return result;
    }

    private static void AddNode(List<Node> results, Stack<Node> nodeStack, Node nodeToAdd)
    {
        if (nodeStack.Count > 0)
        {
            Node parentNode = nodeStack.Peek();
            AddToParent(parentNode, nodeToAdd);
        }
        else
        {
            results.Add(nodeToAdd);
        }
    }

    private static void AddToParent(Node parentNode, Node childNode)
    {
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
