using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Perf.Core;
using Perf.Core.Exporters;
using Perf.Core.Tracing;

namespace Perf.Core.Tests;

public class HtmlExporterTests : IDisposable
{
    private readonly string _tempDir;

    public HtmlExporterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PerfHtmlTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task HtmlExporter_WritesTraceToFile()
    {
        var trace = CreateDummyTrace();
        var exporter = new HtmlExporter(_tempDir);
        
        await exporter.ExportAsync(trace);

        string expectedPath = Path.Combine(_tempDir, $"trace_{trace.TraceId}.html");
        Assert.True(File.Exists(expectedPath));
        
        string content = await File.ReadAllTextAsync(expectedPath);
        Assert.Contains(trace.TraceId.Value.ToString("D"), content);
        Assert.Contains("<!DOCTYPE html>", content);
        Assert.Contains("gantt-container", content);
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