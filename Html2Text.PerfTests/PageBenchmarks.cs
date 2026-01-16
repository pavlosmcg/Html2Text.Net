using BenchmarkDotNet.Attributes;
using System.Text;

namespace Html2Text.PerfTests;

public class PageBenchmarks
{
    private Dictionary<string, string> _files = null!;

    private static readonly Dictionary<string, string> Files =
        Directory.GetFiles("Samples", "*.html")
            .ToDictionary(f => f, File.ReadAllText);

    public IEnumerable<string> FileNames()
        => Files.Keys;

    [ParamsSource(nameof(FileNames))]
    public string FileName { get; set; } = null!;

    [Benchmark]
    public string ParseHtml()
        => Html2Text.Convert(Files[FileName]);
}
