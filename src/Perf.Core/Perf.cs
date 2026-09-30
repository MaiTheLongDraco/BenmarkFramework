using System;
using Perf.Core.Configuration;

namespace Perf.Core;

public static class Perf
{
    public static PerfOptions Options { get; set; } = new PerfOptions();
    
    public static IClock Clock { get; set; } = StopwatchClock.Instance;
    public static ISpanRecorder? Recorder { get; set; } = new InMemoryRecorder();


    public static IDisposable Measure(string operationName, string? category = null)
    {
        if (!Options.Enabled) return PerfScope.NoOp;

        var opId = OperationRegistry.Register(operationName, category);
        return Measure(opId);
    }

    public static IDisposable Measure(OperationId operationId)
    {
        if (!Options.Enabled) return PerfScope.NoOp;

        var currentContext = PerfContext.Current;
        
        if (currentContext == null && !SamplingEngine.ShouldSample(Options.SampleRate))
        {
            return PerfScope.NoOp;
        }

        return new PerfScope(operationId, currentContext, Clock, Recorder);
    }
}