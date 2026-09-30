using System;
using System.Collections.Generic;
using System.Linq;

namespace Perf.Core.Tracing;

public static class TraceBuilder
{
    public static PerfTrace? Build(IReadOnlyList<PerfSpan> spans)
    {
        if (spans == null || spans.Count == 0) return null;

        var traceId = spans[0].TraceId;
        var nodes = spans.ToDictionary(s => s.SpanId, s => new PerfTraceNode(s));
        
        PerfTraceNode? root = null;

        foreach (var span in spans)
        {
            var node = nodes[span.SpanId];
            if (span.ParentSpanId.HasValue && nodes.TryGetValue(span.ParentSpanId.Value, out var parentNode))
            {
                parentNode.Children.Add(node);
            }
            else
            {
                root = node; // Top-level
            }
        }

        if (root == null)
        {
            // Fallback if missing parent
            root = nodes.Values.FirstOrDefault();
        }
        
        // Sort children by start time
        SortChildren(root);

        return root != null ? new PerfTrace(traceId, root) : null;
    }
    
    private static void SortChildren(PerfTraceNode? node)
    {
        if (node == null || node.Children.Count == 0) return;
        node.Children.Sort((a, b) => a.Span.StartTimestamp.CompareTo(b.Span.StartTimestamp));
        foreach (var child in node.Children)
        {
            SortChildren(child);
        }
    }
}