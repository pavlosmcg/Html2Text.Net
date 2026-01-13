using BenchmarkDotNet.Running;

namespace Html2Text.PerfTests;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<PageBenchmarks>();
    }
}
