using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Perf.Core;

namespace Perf.AspNetCore;

public class PerfMiddleware
{
    private readonly RequestDelegate _next;

    public PerfMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get endpoint for normalized route, fallback to path
        var endpoint = context.GetEndpoint();
        string operationName;
        
        if (endpoint is Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint)
        {
            operationName = $"HTTP {context.Request.Method} {routeEndpoint.RoutePattern.RawText}";
        }
        else
        {
            // Use path directly if no endpoint is found (could cause high cardinality if not careful)
            operationName = $"HTTP {context.Request.Method} {context.Request.Path}";
        }

        using var scope = (PerfScope)Perf.Core.Perf.Measure(operationName, "HTTP.Server");
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            scope.MarkFailed(ex);
            throw;
        }
    }
}