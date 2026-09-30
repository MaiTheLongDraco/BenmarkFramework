using System.IO;
using System.Text;
using System.Threading.Tasks;
using Perf.Core.Tracing;

namespace Perf.Core.Exporters;

public class CsvExporter : IPerfExporter
{
    private readonly string _filePath;

    public CsvExporter(string filePath)
    {
        _filePath = filePath;
    }

    public async Task ExportAsync(PerfTrace trace)
    {
        var sb = new StringBuilder();
        
        // Write header if file doesn't exist or is empty
        if (!File.Exists(_filePath) || new FileInfo(_filePath).Length == 0)
        {
            sb.AppendLine("TraceId,SpanId,ParentSpanId,OperationName,StartMs,DurationMs,Status,WaitReason");
        }

        FlattenAndWrite(sb, trace.Root);

        // Simple append, in production use channel/batching for IO
        using var stream = new StreamWriter(_filePath, append: true);
        await stream.WriteAsync(sb.ToString());
    }

    private void FlattenAndWrite(StringBuilder sb, PerfTraceNode node)
    {
        string nodeName = "Unknown";
        if (OperationRegistry.TryGetMetadata(node.Span.OperationId, out var metadata))
        {
            nodeName = metadata.Name;
        }

        sb.AppendLine($"{node.Span.TraceId},{node.Span.SpanId},{node.Span.ParentSpanId},\"{nodeName}\",{node.Span.StartTimestamp},{node.Span.Duration.TotalMilliseconds},{node.Span.Status},{node.Span.WaitReason}");

        foreach (var child in node.Children)
        {
            FlattenAndWrite(sb, child);
        }
    }
}