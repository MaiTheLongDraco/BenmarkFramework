namespace Perf.Core;

public readonly struct TraceId : IEquatable<TraceId>
{
    // For simplicity, using a Guid as TraceId
    public Guid Value { get; }
    
    public TraceId(Guid value) { Value = value; }
    
    public static TraceId Generate() => new(Guid.NewGuid());
    
    public override bool Equals(object? obj) => obj is TraceId id && Equals(id);
    public bool Equals(TraceId other) => Value.Equals(other.Value);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(TraceId left, TraceId right) => left.Equals(right);
    public static bool operator !=(TraceId left, TraceId right) => !left.Equals(right);
    public override string ToString() => Value.ToString("N");
}

public readonly struct SpanId : IEquatable<SpanId>
{
    // Using ulong for SpanId (fast, allocation-free generation possible)
    public ulong Value { get; }
    
    public SpanId(ulong value) { Value = value; }
    
    private static long _counter = 0;
    public static SpanId Generate() => new((ulong)Interlocked.Increment(ref _counter));
    
    public override bool Equals(object? obj) => obj is SpanId id && Equals(id);
    public bool Equals(SpanId other) => Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(SpanId left, SpanId right) => left.Equals(right);
    public static bool operator !=(SpanId left, SpanId right) => !left.Equals(right);
    public override string ToString() => Value.ToString("x16");
}