using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Perf.Core.Metrics;

namespace Perf.Core.Benchmarks;

public static class BenchmarkRunner
{
    public static BenchmarkReport Run(string name, Action action, int warmupIterations = 100, int targetIterations = 1000)
    {
        // Warmup
        for (int i = 0; i < warmupIterations; i++)
        {
            action();
        }

        var histogram = new Histogram();
        var sw = new Stopwatch();

        long startAlloc = GC.GetAllocatedBytesForCurrentThread();
        int startG0 = GC.CollectionCount(0);
        int startG1 = GC.CollectionCount(1);
        int startG2 = GC.CollectionCount(2);

        for (int i = 0; i < targetIterations; i++)
        {
            sw.Restart();
            action();
            sw.Stop();
            histogram.Record(sw.Elapsed.TotalMilliseconds);
        }

        long endAlloc = GC.GetAllocatedBytesForCurrentThread();
        int endG0 = GC.CollectionCount(0);
        int endG1 = GC.CollectionCount(1);
        int endG2 = GC.CollectionCount(2);

        return new BenchmarkReport(
            name, 
            histogram.GetStatistics(), 
            endAlloc - startAlloc, 
            endG0 - startG0, 
            endG1 - startG1, 
            endG2 - startG2);
    }
    
    public static async Task<BenchmarkReport> RunAsync(string name, Func<Task> action, int warmupIterations = 100, int targetIterations = 1000)
    {
        // Warmup
        for (int i = 0; i < warmupIterations; i++)
        {
            await action();
        }

        var histogram = new Histogram();
        var sw = new Stopwatch();

        long startAlloc = GC.GetAllocatedBytesForCurrentThread();
        int startG0 = GC.CollectionCount(0);
        int startG1 = GC.CollectionCount(1);
        int startG2 = GC.CollectionCount(2);

        for (int i = 0; i < targetIterations; i++)
        {
            sw.Restart();
            await action();
            sw.Stop();
            histogram.Record(sw.Elapsed.TotalMilliseconds);
        }

        long endAlloc = GC.GetAllocatedBytesForCurrentThread();
        int endG0 = GC.CollectionCount(0);
        int endG1 = GC.CollectionCount(1);
        int endG2 = GC.CollectionCount(2);

        return new BenchmarkReport(
            name, 
            histogram.GetStatistics(), 
            endAlloc - startAlloc, 
            endG0 - startG0, 
            endG1 - startG1, 
            endG2 - startG2);
    }
}