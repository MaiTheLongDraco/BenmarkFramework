using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Perf.Core;
using Perf.Core.Configuration;

namespace Perf.AspNetCore;

public static class PerfExtensions
{
    public static IServiceCollection AddPerfFramework(this IServiceCollection services, Action<PerfOptions>? configure = null)
    {
        var options = new PerfOptions();
        configure?.Invoke(options);
        
        Perf.Core.Perf.Options = options;
        
        return services;
    }

    public static IApplicationBuilder UsePerf(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerfMiddleware>();
    }
}