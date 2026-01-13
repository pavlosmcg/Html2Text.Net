using BenchmarkDotNet.Attributes;
using System.Text;

namespace Html2Text.PerfTests;

public class PageBenchmarks
{
    private string GetTextFromFile(string filename)
    {
        using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var html = reader.ReadToEnd();
        
        var rawText = Html2Text.Convert(html);
        
        return rawText;
    }

    [Benchmark]
    public string Clampdown() => GetTextFromFile("Samples/clampdown.html");

    [Benchmark]
    public string StackOverflow() => GetTextFromFile("Samples/stackoverflow.html");
}
