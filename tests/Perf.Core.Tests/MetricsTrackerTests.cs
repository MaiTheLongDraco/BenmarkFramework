using System;
using System.Linq;
using Xunit;
using Perf.Core;

namespace Perf.Core.Tests;

public class MetricsTrackerTests
{
    public MetricsTrackerTests()
    {
        Perf.Options.Enabled = true;
        Perf.Options.CaptureAllocations = true;
        Perf.Recorder = new InMemoryRecorder();
    }

    [Fact]
    public void AllocationTracker_MeasuresCorrectly()
    {
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        using (Perf.Measure("AllocatingOperation"))
        {
            // Allocate roughly 100KB
            var bytes = new byte[100 * 1024];
            
            // Do something with it to prevent optimization
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 1;
            }
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Single(spans);
        
        var span = spans[0];
        
        // Assert that the allocated bytes are at least 100KB. 
        // We might allocate slightly more due to object header and array overhead.
        Assert.True(span.AllocatedBytes >= 100 * 1024, $"Expected at least 102400 bytes, got {span.AllocatedBytes}");
        
        // The overhead shouldn't be massive.
        Assert.True(span.AllocatedBytes < 150 * 1024, $"Expected less than 153600 bytes, got {span.AllocatedBytes}");
    }

    [Fact]
    public void DisabledAllocationTracking_ReturnsZero()
    {
        Perf.Options.CaptureAllocations = false;
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        using (Perf.Measure("AllocatingOperation"))
        {
            var bytes = new byte[1024];
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Single(spans);
        
        Assert.Equal(0, spans[0].AllocatedBytes);
        
        // Reset for other tests
        Perf.Options.CaptureAllocations = true;
    }
}
