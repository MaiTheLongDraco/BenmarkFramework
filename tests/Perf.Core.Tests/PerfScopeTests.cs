using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Perf.Core;

namespace Perf.Core.Tests;

public class PerfScopeTests
{
    public PerfScopeTests()
    {
        Perf.Options.Enabled = true;
        Perf.Recorder = new InMemoryRecorder();
    }

    [Fact]
    public void BasicMeasurement_CreatesSpan()
    {
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        using (Perf.Measure("Root"))
        {
            // Do some work
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Single(spans);
        
        var span = spans[0];
        Assert.Null(span.ParentSpanId);
        Assert.True(span.Duration.TotalMilliseconds >= 0);
        Assert.Equal(SpanStatus.Success, span.Status);
    }

    [Fact]
    public void NestedMeasurement_CreatesHierarchy()
    {
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        using (Perf.Measure("Root"))
        {
            using (Perf.Measure("Child"))
            {
                // Child work
            }
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Equal(2, spans.Count);
        
        var childSpan = spans.First(s => s.ParentSpanId != null);
        var rootSpan = spans.First(s => s.ParentSpanId == null);
        
        Assert.Equal(rootSpan.SpanId, childSpan.ParentSpanId);
        Assert.Equal(rootSpan.TraceId, childSpan.TraceId);
    }
    
    [Fact]
    public void Exception_WithManualMark_RecordsFailedStatus()
    {
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        try
        {
            using var scope = (PerfScope)Perf.Measure("FailingOperation");
            try 
            {
                throw new InvalidOperationException("Test error");
            }
            catch (Exception ex)
            {
                scope.MarkFailed(ex);
                throw;
            }
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Single(spans);
        
        var span = spans[0];
        Assert.Equal(SpanStatus.Failed, span.Status);
        Assert.NotNull(span.Exception);
    }
}
