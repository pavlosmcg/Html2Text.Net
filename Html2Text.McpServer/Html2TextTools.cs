using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Html2Text.McpServer;

[McpServerToolType]
public class Html2TextTools
{
    [McpServerTool, Description("Convert an HTML string to plain readable text")]
    public static string ConvertHtmlToText([Description("HTML input to convert")] string html)
    {
        return Html2Text.Convert(html);
    }
}