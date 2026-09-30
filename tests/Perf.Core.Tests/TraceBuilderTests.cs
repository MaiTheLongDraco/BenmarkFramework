using System;
using System.Collections.Generic;
using Xunit;
using Perf.Core;
using Perf.Core.Tracing;
using Perf.Core.Exporters;

namespace Perf.Core.Tests;

public class TraceBuilderTests
{
    [Fact]
    public void Build_ConstructsTreeCorrectly()
    {
        var traceId = TraceId.Generate();
        var rootSpan = SpanId.Generate();
        var child1 = SpanId.Generate();
        var child2 = SpanId.Generate();
        var grandchild = SpanId.Generate();

        var spans = new List<PerfSpan>
        {
            new PerfSpan { TraceId = traceId, SpanId = rootSpan, ParentSpanId = null, StartTimestamp = 1 },
            new PerfSpan { TraceId = traceId, SpanId = child1, ParentSpanId = rootSpan, StartTimestamp = 2 },
            new PerfSpan { TraceId = traceId, SpanId = child2, ParentSpanId = rootSpan, StartTimestamp = 3 },
            new PerfSpan { TraceId = traceId, SpanId = grandchild, ParentSpanId = child1, StartTimestamp = 4 }
        };

        var trace = TraceBuilder.Build(spans);

        Assert.NotNull(trace);
        Assert.Equal(traceId, trace.TraceId);
        Assert.Equal(rootSpan, trace.Root.Span.SpanId);
        Assert.Equal(2, trace.Root.Children.Count);
        
        // Children are sorted by StartTimestamp
        Assert.Equal(child1, trace.Root.Children[0].Span.SpanId);
        Assert.Equal(child2, trace.Root.Children[1].Span.SpanId);
        
        Assert.Single(trace.Root.Children[0].Children);
        Assert.Equal(grandchild, trace.Root.Children[0].Children[0].Span.SpanId);
    }
}
