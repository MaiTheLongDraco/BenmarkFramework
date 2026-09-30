using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Perf.Core.Tracing;

namespace Perf.Core.Exporters;

[JsonSerializable(typeof(PerfTrace))]
[JsonSerializable(typeof(PerfTraceNode))]
[JsonSerializable(typeof(PerfSpan))]
internal partial class PerfTraceJsonContext : JsonSerializerContext
{
}

public class JsonExporter : IPerfExporter
{
    private readonly string _outputDirectory;

    public JsonExporter(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
        Directory.CreateDirectory(_outputDirectory);
    }

    public async Task ExportAsync(PerfTrace trace)
    {
        string filePath = Path.Combine(_outputDirectory, $"trace_{trace.TraceId}.json");
        using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, trace, PerfTraceJsonContext.Default.PerfTrace);
    }
}