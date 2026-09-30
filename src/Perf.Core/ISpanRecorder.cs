using System.Collections.Concurrent;

namespace Perf.Core;

public interface ISpanRecorder
{
    void Record(PerfSpan span);
}

// Simple in-memory recorder for phase 1 testing
public class InMemoryRecorder : ISpanRecorder
{
    private readonly ConcurrentBag<PerfSpan> _spans = new();

    public void Record(PerfSpan span)
    {
        _spans.Add(span);
    }

    public IReadOnlyCollection<PerfSpan> GetSpans() => _spans.ToArray();
    
    public void Clear() => _spans.Clear();
}