using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Perf.Core;
using Perf.Core.Exporters;
using Perf.Core.Tracing;

namespace Perf.Core.Tests;

public class ExportersTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _csvFile;

    public ExportersTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PerfExportersTests_" + Guid.NewGuid().ToString());
        _csvFile = Path.Combine(_tempDir, "traces.csv");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task JsonExporter_WritesTraceToFile()
    {
        var trace = CreateDummyTrace();
        var exporter = new JsonExporter(_tempDir);
        
        await exporter.ExportAsync(trace);

        string expectedPath = Path.Combine(_tempDir, $"trace_{trace.TraceId}.json");
        Assert.True(File.Exists(expectedPath));
        
        string content = await File.ReadAllTextAsync(expectedPath);
        Assert.Contains(trace.TraceId.Value.ToString("D"), content);
    }

    [Fact]
    public async Task CsvExporter_WritesTraceToFile()
    {
        var trace = CreateDummyTrace();
        var exporter = new CsvExporter(_csvFile);
        
        await exporter.ExportAsync(trace);

        Assert.True(File.Exists(_csvFile));
        
        string[] lines = await File.ReadAllLinesAsync(_csvFile);
        Assert.True(lines.Length >= 2); // Header + 1 root span
        Assert.Contains("TraceId,SpanId", lines[0]); // Header
        Assert.Contains(trace.TraceId.ToString(), lines[1]); // Root node
    }

    private PerfTrace CreateDummyTrace()
    {
        var traceId = TraceId.Generate();
        var rootSpan = new PerfSpan { TraceId = traceId, SpanId = SpanId.Generate() };
        var rootNode = new PerfTraceNode(rootSpan);
        return new PerfTrace(traceId, rootNode);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }
}
