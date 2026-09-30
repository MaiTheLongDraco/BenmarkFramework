# 📋 PerfFramework Implementation Checklist

Đây là checklist theo dõi tiến độ của dự án PerfFramework. Bất kỳ Agent nào tham gia dự án đều PHẢI cập nhật file này sau khi hoàn thành một module/task.

---

## 🔴 PHASE 1: Perf.Core (Nền tảng cốt lõi)
- [x] Thiết kế `OperationId` (uint/ulong) và `OperationRegistry`.
- [x] Abstraction `IClock` và implementation mặc định (vd: `StopwatchClock`).
- [x] Thiết kế struct `PerfSpan` (TraceId, SpanId, ParentSpanId, Duration, Status, v.v.).
- [x] Thiết kế `SpanStatus` enum (Success, Failed, Canceled).
- [x] Implementation `PerfScope` (IDisposable, quản lý timer và lifecycle).
- [x] Quản lý state bằng `PerfContext` (`AsyncLocal`).
- [x] Implementation static API `Perf.Measure`.
- [x] Interface `ISpanRecorder` và `InMemoryRecorder` đơn giản.
- [x] Viết test: `PerfScopeTests`, `NestedScopeTests`, `ExceptionInScopeTests`, `ConcurrentScopeTests`.

## 🟠 PHASE 2: Async Context
- [x] Xử lý đúng context flow cho `await`.
- [x] Xử lý context cho `Task.Run` và `ThreadPool`.
- [x] Xử lý context cho parallel tasks (`Parallel.ForEach`, `Task.WhenAll`).
- [x] Viết test: `AsyncAwaitContextTests`, `ThreadSwitchTests`, `TaskRunPropagationTests`, `DeepNestedAsyncTests`.

## 🟡 PHASE 3: Recorder + Span Tree + Statistics
- [x] Xây dựng `SpanRecorder` production-ready (thread-safe batching).
- [x] Implementation `TraceBuilder` (xây dựng cây từ list of spans).
- [x] Tính toán số liệu thống kê: `PerfStatistics` (P50, P90, P99, Max, Min, StdDev).
- [x] Implementation `Histogram` dạng streaming.
- [x] Viết bộ `ConsoleTreeRenderer` hiển thị trace hierarchy ra màn hình.
- [x] Viết test: `SpanRecorderTests`, `TraceBuilderTests`, `StatisticsTests`, `HistogramTests`.

## 🟢 PHASE 4: Source Generator
- [x] Định nghĩa `[Perf]` attribute.
- [x] Xây dựng `PerfSourceGenerator` (Roslyn analyzer).
- [x] Tự động sinh `OperationId` hash lúc compile-time.
- [x] Generate method wrapper (sử dụng partial methods hoặc interceptor).
- [x] Viết test: `SourceGeneratorOutputTests`, `GeneratedInstrumentationTests`, `AOTCompatibilityTests`.

## 🔵 PHASE 5: Metrics + Allocation + GC
- [x] Thêm tính năng đo GC allocation (`GC.GetAllocatedBytesForCurrentThread`).
- [x] Track GC collection count.
- [x] Track CPU time (phân biệt rạch ròi WallClock vs CPUTime, lưu ý cross-platform).
- [x] Track Wait Reason.
- [x] Viết test: `AllocationTrackingTests`, `CpuTimeTests`, `MetricAggregationTests`.

## 🟣 PHASE 6: Adapters
- [x] Thiết kế `IPerfAdapter` contract.
- [x] **Perf.Sql**: Track DbCommand, loại bỏ sensitive parameters.
- [x] **Perf.Net**: HTTP request, loại bỏ Authorization/Cookie.
- [x] **Perf.Redis**: Track commands.
- [x] **Perf.FileIO**: Stream reading/writing, tính toán bytes throughput.
- [x] **Lock**: Helper cho SemaphoreSlim, Monitor, Lock.
- [x] Viết test: Integration tests cho từng adapter.

## ⚪ PHASE 7: Unity Adapter
- [x] Mapping sang `ProfilerMarker` của Unity.
- [x] Measure `Update`, `LateUpdate`, PlayerLoop.
- [x] Wrapper cho `AsyncOperation` và `Addressables`.
- [x] Viết test và Benchmark tương thích AOT / IL2CPP.

## 🟤 PHASE 8: Exporters
- [x] Thiết kế `IPerfExporter` interface.
- [x] Viết `JsonExporter`.
- [x] Viết `CsvExporter`.
- [x] Viết `OpenTelemetryExporter`.
- [x] Viết test.

## ⚫ PHASE 9: Benchmark Engine
- [x] Viết engine warmup, iteration runner.
- [x] Đo overhead của chính PerfFramework (Enable vs Disable).
- [x] Xuất báo cáo benchmark.
- [x] Viết test.

## 🔶 PHASE 10: Configuration & Security
- [x] Xây dựng mô hình `PerfOptions`.
- [x] Implementation `SamplingEngine` (theo rate, slow requests).
- [x] Redaction/Masking data cho params, bodies.
- [x] Viết test.

## 🔷 PHASE 11: ASP.NET Core
- [x] Middleware extension `app.UsePerf()`.
- [x] Request trace hierarchy test.

## 🔸 PHASE 12: Dashboard
- [x] Sinh HTML report đơn giản, Timeline view.
