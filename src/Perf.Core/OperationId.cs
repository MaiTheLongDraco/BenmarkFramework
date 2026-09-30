namespace Perf.Core;

/// <summary>
/// A high-performance, strongly-typed identity for an operation, designed to avoid string allocations on the hot path.
/// </summary>
public readonly struct OperationId : IEquatable<OperationId>
{
    public ulong Value { get; }

    public OperationId(ulong value)
    {
        Value = value;
    }

    public override bool Equals(object? obj) => obj is OperationId id && Equals(id);
    public bool Equals(OperationId other) => Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(OperationId left, OperationId right) => left.Equals(right);
    public static bool operator !=(OperationId left, OperationId right) => !left.Equals(right);
    
    public override string ToString() => Value.ToString();
}

public record OperationMetadata(
    OperationId Id,
    string Name,
    string? Category = null
);