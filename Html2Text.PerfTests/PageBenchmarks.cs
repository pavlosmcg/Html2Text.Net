using BenchmarkDotNet.Attributes;

namespace Html2Text.PerfTests;

[ShortRunJob]
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
public class PageBenchmarks
{
    private string _html = null!;

    public IEnumerable<string?> FileNames()
        => Directory
            .GetFiles("Samples", "*.html")
            .Select(Path.GetFileNameWithoutExtension);

    [ParamsSource(nameof(FileNames))]
    public string FileName { get; set; } = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _html = File.ReadAllText($"Samples/{FileName}.html");
    }

    [Benchmark]
    public string Convert() => Html2Text.Convert(_html);
}
