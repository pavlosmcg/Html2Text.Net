using System.ComponentModel;
using System.IO;
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

    public sealed record ConvertHtmlFileToTextResult(
        string OutputFilePath,
        int InputChars,
        int OutputChars);

    [McpServerTool, Description("Convert an HTML file to plain readable text and write it next to the input file with a .txt extension.")]
    public static ConvertHtmlFileToTextResult ConvertHtmlFileToText(
        [Description("Path to the input HTML file")] string inputFilePath)
    {
        if (string.IsNullOrWhiteSpace(inputFilePath))
            throw new ArgumentException("Input file path is required.", nameof(inputFilePath));

        // Normalize/validate input
        var inputFullPath = Path.GetFullPath(inputFilePath);
        if (!File.Exists(inputFullPath))
            throw new FileNotFoundException("Input file was not found.", inputFullPath);

        var inputExt = Path.GetExtension(inputFullPath);
        if (!inputExt.Equals(".html", StringComparison.OrdinalIgnoreCase) &&
            !inputExt.Equals(".htm", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported input file extension '{inputExt}'. Expected .html or .htm.");
        }

        // Output path next to input, appending .txt
        var outputFullPath = inputFullPath + ".txt";

        var html = File.ReadAllText(inputFullPath);
        var text = Html2Text.Convert(html);

        File.WriteAllText(outputFullPath, text);

        return new ConvertHtmlFileToTextResult(
            OutputFilePath: outputFullPath,
            InputChars: html.Length,
            OutputChars: text.Length);
    }
}
