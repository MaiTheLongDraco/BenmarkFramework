using System;
using System.Threading.Tasks;
using Xunit;
using Perf.Core;
using Perf.Core.Runtime;

namespace Perf.Core.Tests;

public class SpanRecorderTests
{
    [Fact]
    public async Task AsyncSpanRecorder_RecordsAndReadsSpans()
    {
        using var recorder = new AsyncSpanRecorder(capacity: 10);
        var span = new PerfSpan { SpanId = SpanId.Generate() };
        
        recorder.Record(span);
        
        var readSpan = await recorder.Reader.ReadAsync();
        Assert.Equal(span.SpanId, readSpan.SpanId);
    }
    
    [Fact]
    public void AsyncSpanRecorder_DropsOldestWhenFull()
    {
        using var recorder = new AsyncSpanRecorder(capacity: 2);
        
        recorder.Record(new PerfSpan { StartTimestamp = 1 });
        recorder.Record(new PerfSpan { StartTimestamp = 2 });
        recorder.Record(new PerfSpan { StartTimestamp = 3 }); // This should drop the first one
        
        // Let's read
        Assert.True(recorder.Reader.TryRead(out var span1));
        Assert.Equal(2, span1.StartTimestamp); // First one was dropped
        
        Assert.True(recorder.Reader.TryRead(out var span2));
        Assert.Equal(3, span2.StartTimestamp);
        
        Assert.False(recorder.Reader.TryRead(out _)); // Empty
    }
}
