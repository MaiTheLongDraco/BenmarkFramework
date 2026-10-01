#nullable enable
using System;
using System.Collections.Concurrent;
using Perf.Core;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEngine.Profiling;
#endif

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
        public static long GetMonoUsedSizeLong() => 0;
    }
}
#endif

namespace Perf.Unity
{

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

    public struct UnityScope : IDisposable
    {
        private readonly PerfScope? _coreScope;
        private readonly long _startMemory;

        public UnityScope(PerfScope? coreScope, string operationName)
        {
            _coreScope = coreScope;
            Profiler.BeginSample(operationName);
            _startMemory = Profiler.GetMonoUsedSizeLong();
        }

        public void Dispose()
        {
            Profiler.EndSample();
            if (_coreScope != null)
            {
                // Thay thế bộ đếm RAM của Core bằng bộ đếm RAM của Unity
                long endMemory = Profiler.GetMonoUsedSizeLong();
                long allocated = endMemory - _startMemory;
                
                if (allocated > 0)
                {
                    _coreScope.MarkAllocatedBytes(allocated);
                }
                
                _coreScope.Dispose();
            }
        }
    }

    public static class PerfUnity
    {
        public static UnityScope Measure(string operationName)
        {
            var opId = OperationRegistry.Register(operationName);
            var coreScope = Perf.Core.Perf.Measure(operationName) as PerfScope;
            return new UnityScope(coreScope, operationName);
        }

        public static ProfilerMarker GetMarker(OperationId opId)
        {
            if (OperationRegistry.TryGetMetadata(opId, out var meta))
            {
                return new ProfilerMarker(meta.Name);
            }
            return new ProfilerMarker("Unknown");
        }
    }
}