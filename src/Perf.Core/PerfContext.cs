namespace Perf.Core;

public sealed class PerfContext
{
    public TraceId TraceId { get; }
    public SpanId CurrentSpanId { get; }
    
    public PerfContext(TraceId traceId, SpanId currentSpanId)
    {
        TraceId = traceId;
        CurrentSpanId = currentSpanId;
    }

    private static readonly AsyncLocal<PerfContext?> _current = new();

    public static PerfContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}