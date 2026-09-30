using System;
using System.Text;
using Perf.Core.Tracing;

namespace Perf.Core.Exporters;

public class ConsoleTreeRenderer
{
    public string Render(PerfTrace trace)
    {
        var sb = new StringBuilder();
        RenderNode(sb, trace.Root, "", true);
        return sb.ToString();
    }

    private void RenderNode(StringBuilder sb, PerfTraceNode node, string indent, bool isLast)
    {
        string nodeName = "Unknown";
        if (OperationRegistry.TryGetMetadata(node.Span.OperationId, out var metadata))
        {
            nodeName = metadata.Name;
        }

        string branch = indent.Length == 0 ? "" : (isLast ? "└── " : "├── ");
        
        sb.AppendLine($"{indent}{branch}{nodeName} \t {node.Span.Duration.TotalMilliseconds:F2} ms");

        string nextIndent = indent + (indent.Length == 0 ? "" : (isLast ? "    " : "│   "));
        
        for (int i = 0; i < node.Children.Count; i++)
        {
            RenderNode(sb, node.Children[i], nextIndent, i == node.Children.Count - 1);
        }
    }
}