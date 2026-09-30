using System;
using System.Threading.Tasks;
using Xunit;
using Perf.Core;
using Perf.Core.Benchmarks;
using Xunit.Abstractions;

namespace Perf.Core.Tests;

public class BenchmarkTests
{
    private readonly ITestOutputHelper _output;
    
    public BenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void BenchmarkRunner_RunsAndGeneratesReport()
    {
        Perf.Options.Enabled = true;
        Perf.Options.CaptureAllocations = true;
        Perf.Recorder = new InMemoryRecorder();

        int sum = 0;
        var report = BenchmarkRunner.Run("TestLoop", () => 
        {
            using (Perf.Measure("Inner"))
            {
                sum += 1;
            }
        }, warmupIterations: 10, targetIterations: 100);

        Assert.NotNull(report);
        Assert.Equal("TestLoop", report.Name);
        Assert.Equal(100, report.Statistics.Count);
        
        _output.WriteLine($"Allocated bytes per op: {report.TotalAllocatedBytes / 100.0}");
    }
}
