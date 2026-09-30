using System;
using Perf.Core.Metrics;

namespace Perf.Core.Benchmarks;

public class BenchmarkReport
{
    public string Name { get; }
    public PerfStatistics Statistics { get; }
    public long TotalAllocatedBytes { get; }
    public long Gen0Collections { get; }
    public long Gen1Collections { get; }
    public long Gen2Collections { get; }

    public BenchmarkReport(
        string name, 
        PerfStatistics stats, 
        long allocatedBytes, 
        long gen0, 
        long gen1, 
        long gen2)
    {
        Name = name;
        Statistics = stats;
        TotalAllocatedBytes = allocatedBytes;
        Gen0Collections = gen0;
        Gen1Collections = gen1;
        Gen2Collections = gen2;
    }

    public void PrintToConsole()
    {
        Console.WriteLine($"--- Benchmark: {Name} ---");
        Console.WriteLine($"Iterations: {Statistics.Count}");
        Console.WriteLine($"Avg: {Statistics.AvgMs:F4} ms");
        Console.WriteLine($"Min: {Statistics.MinMs:F4} ms | Max: {Statistics.MaxMs:F4} ms");
        Console.WriteLine($"P50: {Statistics.P50Ms:F4} ms | P99: {Statistics.P99Ms:F4} ms");
        
        double bytesPerOp = Statistics.Count > 0 ? (double)TotalAllocatedBytes / Statistics.Count : 0;
        Console.WriteLine($"Allocated: {bytesPerOp:F0} bytes/op");
        Console.WriteLine($"GC Collections: Gen0={Gen0Collections}, Gen1={Gen1Collections}, Gen2={Gen2Collections}");
        Console.WriteLine(new string('-', 30));
    }
}