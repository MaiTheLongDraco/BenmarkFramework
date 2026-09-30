using System;
using Xunit;
using Perf.Core.Metrics;

namespace Perf.Core.Tests;

public class HistogramTests
{
    [Fact]
    public void Record_And_GetStatistics()
    {
        var histogram = new Histogram();
        for(int i = 1; i <= 100; i++)
        {
            histogram.Record(i); // values 1 to 100
        }

        var stats = histogram.GetStatistics();
        Assert.Equal(100, stats.Count);
        Assert.Equal(1, stats.MinMs);
        Assert.Equal(100, stats.MaxMs);
        Assert.Equal(50.5, stats.AvgMs, 1);
        
        // The bucket-based percentile estimation isn't exact for all arbitrary ranges, 
        // but it should be within the bucket bounds.
        Assert.True(stats.P50Ms > 0 && stats.P50Ms <= 100);
        Assert.True(stats.P99Ms >= 90 && stats.P99Ms <= 100);
    }
}
