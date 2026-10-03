using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Perf.Core;
using Perf.Net;

namespace Perf.IntegrationTests;

public class AdaptersTests
{
    public AdaptersTests()
    {
        Perf.Core.Perf.Options.Enabled = true;
        Perf.Core.Perf.Options.CaptureAllocations = false;
        Perf.Core.Perf.Recorder = new InMemoryRecorder();
    }

    [Fact]
    public async Task PerfHttpMessageHandler_RecordsSpan()
    {
        var recorder = (InMemoryRecorder)Perf.Core.Perf.Recorder!;
        recorder.Clear();

        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(new PerfHttpMessageHandler(mockHandler));

        using (Perf.Core.Perf.Measure("Root"))
        {
            await client.GetAsync("http://localhost");
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Equal(2, spans.Count);
        
        var httpSpan = spans.Single(s => s.ParentSpanId != null);
        var rootSpan = spans.Single(s => s.ParentSpanId == null);
        
        Assert.Equal(rootSpan.SpanId, httpSpan.ParentSpanId);
        
        OperationRegistry.TryGetMetadata(httpSpan.OperationId, out var opMeta);
        Assert.NotNull(opMeta);
        Assert.Equal("HTTP.Request", opMeta.Name);
    }

    [Fact]
    public async Task PerfLock_RecordsSpan()
    {
        var recorder = (InMemoryRecorder)Perf.Core.Perf.Recorder!;
        recorder.Clear();
        
        var semaphore = new SemaphoreSlim(1, 1);

        using (Perf.Core.Perf.Measure("Root"))
        {
            using (await PerfLock.MeasureWaitAsync(semaphore))
            {
                // do work
            }
        }

        var spans = recorder.GetSpans().ToList();
        Assert.Equal(2, spans.Count);
        
        var lockSpan = spans.Single(s => s.ParentSpanId != null);
        
        OperationRegistry.TryGetMetadata(lockSpan.OperationId, out var opMeta);
        Assert.NotNull(opMeta);
        Assert.Equal("Lock.SemaphoreSlim", opMeta.Name);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
