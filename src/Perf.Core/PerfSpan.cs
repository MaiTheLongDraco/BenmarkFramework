namespace Perf.Core;

public struct PerfSpan
{
    public TraceId TraceId { get; set; }
    public SpanId SpanId { get; set; }
    public SpanId? ParentSpanId { get; set; }
    public OperationId OperationId { get; set; }
    
    public long StartTimestamp { get; set; }
    public long EndTimestamp { get; set; }
    
    public SpanStatus Status { get; set; }
    public Exception? Exception { get; set; }
    public WaitReason WaitReason { get; set; }
    
    // Metrics
    public long AllocatedBytes { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long? CpuTimeNs { get; set; } // Platform dependent
    
    // Store reference to clock for duration calculation, avoiding allocating during record
    internal IClock? Clock { get; set; }

    public TimeSpan Duration => Clock?.GetElapsedTime(StartTimestamp, EndTimestamp) ?? TimeSpan.Zero;
}