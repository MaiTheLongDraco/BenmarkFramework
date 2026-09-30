using System.Collections.Generic;

namespace Perf.Core.Tracing;

public class PerfTraceNode
{
    public PerfSpan Span { get; set; }
    public List<PerfTraceNode> Children { get; } = new();
    
    public PerfTraceNode(PerfSpan span)
    {
        Span = span;
    }
}

public class PerfTrace
{
    public TraceId TraceId { get; set; }
    public PerfTraceNode Root { get; set; }
    
    public PerfTrace(TraceId traceId, PerfTraceNode root)
    {
        TraceId = traceId;
        Root = root;
    }
}