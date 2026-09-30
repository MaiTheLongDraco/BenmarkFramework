using System;
using System.Threading;

namespace Perf.Core.Configuration;

public static class SamplingEngine
{
    private static readonly ThreadLocal<Random> _random = new(() => new Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId));

    public static bool ShouldSample(double sampleRate)
    {
        if (sampleRate >= 1.0) return true;
        if (sampleRate <= 0.0) return false;
        
        return _random.Value!.NextDouble() < sampleRate;
    }
}