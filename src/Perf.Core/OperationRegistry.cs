using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Perf.Core;

public static class OperationRegistry
{
    private static readonly ConcurrentDictionary<string, OperationMetadata> _nameToMetadata = new();
    private static readonly ConcurrentDictionary<OperationId, OperationMetadata> _idToMetadata = new();

    public static OperationId Register(string name, string? category = null)
    {
        // Simple 64-bit hash (djb2 or similar) or incremental ID
        // For simplicity in Phase 1, we use FNV-1a 64-bit
        ulong hash = CalculateFnv1a64(name);
        var id = new OperationId(hash);
        var metadata = new OperationMetadata(id, name, category);

        _nameToMetadata.TryAdd(name, metadata);
        _idToMetadata.TryAdd(id, metadata);

        return id;
    }

    public static bool TryGetMetadata(OperationId id, [NotNullWhen(true)] out OperationMetadata? metadata)
    {
        return _idToMetadata.TryGetValue(id, out metadata);
    }
    
    public static System.Collections.Generic.IEnumerable<OperationMetadata> GetAllMetadata()
    {
        return _idToMetadata.Values;
    }
    
    private static ulong CalculateFnv1a64(string text)
    {
        ulong hash = 14695981039346656037UL;
        foreach (char c in text)
        {
            hash ^= c;
            hash *= 1099511628211UL;
        }
        return hash;
    }
}