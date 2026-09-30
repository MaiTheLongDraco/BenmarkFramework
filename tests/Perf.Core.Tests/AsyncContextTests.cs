using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Perf.Core;

namespace Perf.Core.Tests;

public class AsyncContextTests
{
    public AsyncContextTests()
    {
        Perf.Options.Enabled = true;
        Perf.Recorder = new InMemoryRecorder();
    }

    [Fact]
    public async Task AsyncAwait_PreservesContextAcrossThreadSwitches()
    {
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        using (Perf.Measure("Root"))
        {
            await Task.Delay(10); // Forces async state machine yield and potential thread switch

            using (Perf.Measure("ChildAfterAwait"))
            {
                await Task.Delay(10);
            }
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Equal(2, spans.Count);
        
        var root = spans.Single(s => s.ParentSpanId == null);
        var child = spans.Single(s => s.ParentSpanId != null);
        
        Assert.Equal(root.SpanId, child.ParentSpanId);
        Assert.Equal(root.TraceId, child.TraceId);
    }

    [Fact]
    public async Task TaskRun_ForksContextCorrectly()
    {
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        using (Perf.Measure("Root"))
        {
            var t1 = Task.Run(async () => 
            {
                using (Perf.Measure("Child1"))
                {
                    await Task.Delay(10);
                }
            });

            var t2 = Task.Run(async () => 
            {
                using (Perf.Measure("Child2"))
                {
                    await Task.Delay(10);
                }
            });

            await Task.WhenAll(t1, t2);
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Equal(3, spans.Count);
        
        var root = spans.Single(s => s.ParentSpanId == null);
        var children = spans.Where(s => s.ParentSpanId != null).ToList();
        
        Assert.Equal(2, children.Count);
        foreach (var child in children)
        {
            Assert.Equal(root.SpanId, child.ParentSpanId);
            Assert.Equal(root.TraceId, child.TraceId);
        }
    }

    [Fact]
    public void ParallelForEach_PreservesContext()
    {
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        using (Perf.Measure("Root"))
        {
            Parallel.For(0, 10, i => 
            {
                using (Perf.Measure("ParallelChild"))
                {
                    // compute something
                }
            });
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Equal(11, spans.Count); // 1 root + 10 children
        
        var root = spans.Single(s => s.ParentSpanId == null);
        var children = spans.Where(s => s.ParentSpanId != null).ToList();
        
        Assert.Equal(10, children.Count);
        foreach (var child in children)
        {
            Assert.Equal(root.SpanId, child.ParentSpanId);
        }
    }

    [Fact]
    public async Task ConfigureAwaitFalse_WorksCorrectly()
    {
        var recorder = (InMemoryRecorder)Perf.Recorder!;
        recorder.Clear();

        using (Perf.Measure("Root"))
        {
            await Task.Delay(10).ConfigureAwait(false);

            using (Perf.Measure("Child"))
            {
                // do work
            }
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Equal(2, spans.Count);
        
        var root = spans.Single(s => s.ParentSpanId == null);
        var child = spans.Single(s => s.ParentSpanId != null);
        
        Assert.Equal(root.SpanId, child.ParentSpanId);
    }
}
