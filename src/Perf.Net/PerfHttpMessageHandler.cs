using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Perf.Core;

namespace Perf.Net;

public class PerfHttpMessageHandler : DelegatingHandler
{
    private readonly OperationId _httpOpId;

    public PerfHttpMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
        _httpOpId = OperationRegistry.Register("HTTP.Request");
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var scope = (PerfScope)Perf.Core.Perf.Measure(_httpOpId);
        try
        {
            // Note: In a full implementation we'd normalize the URL here
            // e.g., string normalizedUrl = NormalizeUrl(request.RequestUri);
            // and attach it as metadata.
            
            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            scope.MarkFailed(ex);
            throw;
        }
    }
}