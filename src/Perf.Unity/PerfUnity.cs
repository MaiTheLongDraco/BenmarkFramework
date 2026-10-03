#nullable enable
using System;
using System.Collections.Concurrent;
using Perf.Core;

// Mocking Unity's Profiler API for demonstration/compilation purposes outside of actual Unity Editor
#if !UNITY_5_3_OR_NEWER
namespace UnityEngine.Profiling
{
    public struct ProfilerMarker
    {
        public ProfilerMarker(string name) { }
        public void Begin() { }
        public void End() { }
    }

    public static class Profiler
    {
        public static void BeginSample(string name) { }
        public static void EndSample() { }
    }
}
#endif

namespace Perf.Unity
{
    using UnityEngine.Profiling;

    public class UnityProfilerRecorder : ISpanRecorder
    {
        // Caching ProfilerMarkers since creating them has overhead
        private readonly ConcurrentDictionary<ulong, ProfilerMarker> _markers = new();
        private readonly ISpanRecorder? _innerRecorder;

        public UnityProfilerRecorder(ISpanRecorder? innerRecorder = null)
        {
            _innerRecorder = innerRecorder;
        }

        public void Record(PerfSpan span)
        {
            // Unity Profiler API requires Begin/End, but ISpanRecorder receives the completed Span.
            // To truly map to Unity's Profiler in real-time, we actually need to hook into PerfScope creation.
            // Since ISpanRecorder is called AFTER the span completes, we can only log it as an event, 
            // OR we need an IPerfObserver that hooks into Begin/End.
            // For Phase 7, we'll design an Event emission or forward to inner.
            _innerRecorder?.Record(span);
        }
    }

    public static class PerfUnity
    {
        public static ProfilerMarker GetMarker(OperationId opId)
        {
            if (OperationRegistry.TryGetMetadata(opId, out var meta))
            {
                return new ProfilerMarker(meta.Name);
            }
            return new ProfilerMarker("Unknown");
        }

        public static IDisposable Measure(string operationName, string? category = null)
        {
            UnityEngine.Profiling.Profiler.BeginSample(operationName);
            var coreScope = Perf.Core.Perf.Measure(operationName, category);
            return new UnityScopeWrapper(coreScope);
        }

        private class UnityScopeWrapper : IDisposable
        {
            private readonly IDisposable _inner;

            public UnityScopeWrapper(IDisposable inner)
            {
                _inner = inner;
            }

            public void Dispose()
            {
                _inner.Dispose();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
    }
}