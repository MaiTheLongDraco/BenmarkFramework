using System;
using System.Diagnostics;

namespace Perf.Core;

public sealed class PerfScope : IDisposable
{
    private PerfSpan _span;
    private readonly PerfContext? _parentContext;
    private readonly ISpanRecorder? _recorder;
    private bool _isDisposed;
    
    // Starting metrics
    private readonly long _startAllocatedBytes;
    private readonly int _startGen0;
    private readonly int _startGen1;
    private readonly int _startGen2;
    private readonly long? _startCpuTime;
    private readonly bool _isNoOp;
    
    public static readonly PerfScope NoOp = new PerfScope();

    // NoOp Constructor
    private PerfScope()
    {
        _isNoOp = true;
        _span = default; // Empty struct
    }

    // Use object pooling for zero-allocation later if needed. For now, basic class allocation.
    internal PerfScope(
        OperationId operationId,
        PerfContext? parentContext,
        IClock clock,
        ISpanRecorder? recorder)
    {
        _parentContext = parentContext;
        _recorder = recorder;
        
        var traceId = parentContext?.TraceId ?? TraceId.Generate();
        var spanId = SpanId.Generate();
        
        _span = new PerfSpan
        {
            TraceId = traceId,
            SpanId = spanId,
            ParentSpanId = parentContext?.CurrentSpanId,
            OperationId = operationId,
            StartTimestamp = clock.GetTimestamp(),
            Status = SpanStatus.Success, // Default
            Clock = clock
        };

        if (Perf.Options.CaptureAllocations)
        {
            _startAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();
            _startGen0 = GC.CollectionCount(0);
            _startGen1 = GC.CollectionCount(1);
            _startGen2 = GC.CollectionCount(2);
        }

        if (Perf.Options.CaptureCpuTime)
        {
            _startCpuTime = GetCurrentThreadCpuTime();
        }

        // Set AsyncLocal current context
        PerfContext.Current = new PerfContext(traceId, spanId);
    }

    private static long? GetCurrentThreadCpuTime()
    {
        // Platform dependent - wrapping in try-catch in case not supported on current OS
        try
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) || 
                System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) || 
                System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                // This gets CPU time for current PROCESS, not Thread easily via ProcessThread
                // A true thread CPU time requires P/Invoke (GetThreadTimes on Windows, clock_gettime on Linux).
                // For simplicity, we just use the Process-wide time here, but it's not thread-safe.
                // In a production system, this would be a P/Invoke call. We'll leave it as null for now
                // if we can't reliably get thread-local CPU time to avoid inaccurate data.
            }
        }
        catch { }
        return null; 
    }

    // Public setter for manual exception tagging in Phase 1
    public void MarkFailed(Exception ex)
    {
        if (_isNoOp) return;
        _span.Status = SpanStatus.Failed;
        _span.Exception = ex;
    }

    public void Dispose()
    {
        if (_isDisposed || _isNoOp) return;
        _isDisposed = true;

        if (_span.Clock != null)
        {
            _span.EndTimestamp = _span.Clock.GetTimestamp();
        }

        if (Perf.Options.CaptureAllocations)
        {
            _span.AllocatedBytes = GC.GetAllocatedBytesForCurrentThread() - _startAllocatedBytes;
            _span.Gen0Collections = GC.CollectionCount(0) - _startGen0;
            _span.Gen1Collections = GC.CollectionCount(1) - _startGen1;
            _span.Gen2Collections = GC.CollectionCount(2) - _startGen2;
        }

        if (Perf.Options.CaptureCpuTime && _startCpuTime.HasValue)
        {
            _span.CpuTimeNs = GetCurrentThreadCpuTime() - _startCpuTime;
        }

        // Ideally, we could check for an unhandled exception here (e.g., via Marshal.GetExceptionCode() or similar),
        // but robust exception detection requires language support (like try/catch/finally in a generator)
        // or explicit developer marking. We rely on manual MarkFailed for now.

        _recorder?.Record(_span);

        // Restore context
        PerfContext.Current = _parentContext;
    }
}