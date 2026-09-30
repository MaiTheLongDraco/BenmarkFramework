namespace Perf.Core.Metrics;

public readonly struct PerfStatistics
{
    public long Count { get; }
    public double MinMs { get; }
    public double MaxMs { get; }
    public double AvgMs { get; }
    public double P50Ms { get; }
    public double P90Ms { get; }
    public double P95Ms { get; }
    public double P99Ms { get; }

    public PerfStatistics(long count, double minMs, double maxMs, double avgMs, double p50Ms, double p90Ms, double p95Ms, double p99Ms)
    {
        Count = count;
        MinMs = minMs;
        MaxMs = maxMs;
        AvgMs = avgMs;
        P50Ms = p50Ms;
        P90Ms = p90Ms;
        P95Ms = p95Ms;
        P99Ms = p99Ms;
    }
}