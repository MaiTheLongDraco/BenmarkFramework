using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Perf.Core;
using Perf.AspNetCore;

namespace Perf.IntegrationTests;

public class AspNetCoreTests
{
    [Fact]
    public async Task Middleware_RecordsRequestSpan()
    {
        var recorder = new InMemoryRecorder();
        
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPerfFramework(options => 
                {
                    options.Enabled = true;
                    options.CaptureAllocations = true;
                });
            })
            .Configure(app =>
            {
                // Override recorder for tests
                Perf.Core.Perf.Recorder = recorder;
                
                app.UseRouting();
                app.UsePerf();
                
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/api/test", async context =>
                    {
                        await context.Response.WriteAsync("Hello World");
                    });
                });
            });

        using var server = new TestServer(builder);
        var client = server.CreateClient();

        var response = await client.GetAsync("/api/test");
        response.EnsureSuccessStatusCode();

        var spans = recorder.GetSpans();
        Assert.Single(spans);
        
        var span = spans.First();
        Assert.True(span.Duration.TotalMilliseconds >= 0);
        
        OperationRegistry.TryGetMetadata(span.OperationId, out var meta);
        Assert.Equal("HTTP GET /api/test", meta.Name);
        Assert.Equal("HTTP.Server", meta.Category);
    }
}