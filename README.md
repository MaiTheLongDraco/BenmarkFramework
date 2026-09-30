# 🚀 PerfFramework - Khung Đo Lường Hiệu Năng Vô Cực (Zero-Allocation)

PerfFramework là một bộ công cụ đo lường hiệu năng (Telemetry & Profiling) được thiết kế theo nguyên tắc tối giản (Simplicity First) của Andrej Karpathy. Nó tập trung vào sự tinh gọn, không cấp phát bộ nhớ (Zero-Allocation) trên hot path, hoàn toàn tương thích với AOT (Ahead-Of-Time) biên dịch và IL2CPP của Unity.

## 🌟 Các Tính Năng Cốt Lõi
- **Zero-Allocation**: Đo lường bằng struct và AsyncLocal, giải phóng áp lực cho Garbage Collector.
- **Tương thích Unity IL2CPP**: Xây dựng trên nền tảng .NET Standard 2.1, không dùng Reflection.Emit, bọc thẳng vào ProfilerMarker.
- **AOT & Source Generators**: Hoàn toàn an toàn cho Native AOT. Hashing tĩnh tại thời điểm biên dịch.
- **Micro-Benchmark Engine**: Tích hợp sẵn bộ đo overhead siêu nhẹ với Warmup phase.
- **Bảo Mật PII**: Tự động bóc tách và ẩn giấu các thông tin nhạy cảm (Thẻ tín dụng, JWT, Email).
- **Xuất Báo Cáo Đa Dạng**: Xuất Console Tree, JSON, CSV và HTML Gantt Chart Timeline cực kỳ trực quan.

---

## 🛠️ Hướng Dẫn Sử Dụng (Cho C# .NET Solutions)

### 1. Cấu hình cơ bản (Program.cs)
Bật Tracer và gắn cấu hình exporter (ví dụ in ra Console).

`csharp
using Perf.Core;
using Perf.Core.Configuration;
using Perf.Core.Runtime;

// Bật Tracer và theo dõi cấp phát GC, CPU Time
Perf.Core.Perf.Options = new PerfOptions 
{ 
    Enabled = true, 
    CaptureAllocations = true, 
    CaptureCpuTime = true,
    SampleRate = 1.0 // 100% requests
};

// Chuyển recorder mặc định sang AsyncSpanRecorder (thread-safe, high performance)
Perf.Core.Perf.Recorder = new AsyncSpanRecorder(batchSize: 100);
`

### 2. Đo lường bất kỳ Block Code nào (Manual Tracing)

Chỉ cần bọc khối code của bạn vào using var scope = Perf.Measure(). Nó hoàn toàn thread-safe và support async/await.

`csharp
public async Task ProcessOrderAsync(int orderId)
{
    using var scope = Perf.Measure("ProcessOrder");
    try 
    {
        await ValidateOrderAsync(); // Span con
        await SaveToDatabaseAsync(); // Span con
    }
    catch (Exception ex)
    {
        scope.MarkFailed(ex);
        throw;
    }
}
`

### 3. Tích hợp ASP.NET Core Middleware

Chỉ cần gọi AddPerfFramework() và UsePerf(). Toàn bộ HTTP Request sẽ được tự động gom nhóm, gộp Route và track thời gian response.

`csharp
using Perf.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPerfFramework(options => 
{
    options.Enabled = true;
    options.CaptureAllocations = true;
});

var app = builder.Build();

app.UsePerf(); // <--- Đặt trước UseEndpoints

app.MapGet("/api/users/{id}", async (int id) => {
    // Bên trong này bạn có thể gọi thêm Perf.Measure()
    // Nó sẽ tự động được gán làm Span con (Child) của request HTTP hiện tại.
    return Results.Ok();
});

app.Run();
`

---

## 🎮 Hướng Dẫn Sử Dụng (Cho Unity 3D)

Unity không dùng ASP.NET hay Console, và hệ thống Coroutine/Update loop cần cách tiếp cận đặc thù. PerfFramework giải quyết việc này bằng Adapter Perf.Unity.

### 1. Tích Hợp Vào Unity Profiler Window
Khi bạn đo lường trên PerfFramework, nó tự động bắn tín hiệu xuống Profiler của Unity để bạn xem trực tiếp trong Unity Editor (bằng ProfilerMarker).

`csharp
using Perf.Core;
using Perf.Unity;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Trỏ Recorder tới Unity Profiler
        Perf.Core.Perf.Recorder = new UnityProfilerRecorder();
    }

    void Update()
    {
        // Đo đạc một vòng loop
        using (Perf.Measure("GameManager.UpdateLogic"))
        {
            CalculatePhysics();
            SpawnEnemies();
        }
    }
}
`

### 2. Xử lý UniTask / Async trong Unity
Unity hỗ trợ chuẩn sync/await thông qua UniTask hoặc System.Threading.Tasks. PerfFramework tự động chuyển flow ngữ cảnh cha-con (Parent-Child) chính xác qua từng frame.

`csharp
public async Cysharp.Threading.Tasks.UniTask LoadLevelAsync()
{
    using (Perf.Measure("LevelLoading"))
    {
        await LoadAssetsAsync(); // Chạy ngầm, không block main thread
        await InstantiatePrefabsAsync();
    }
}
`

---

## 🗄️ Hướng Dẫn Sử Dụng (Cho Các Database/HTTP Adapters)

PerfFramework đi kèm với bộ Adapter tự động đo lường mọi truy vấn SQL và HTTP Request đi ra khỏi ứng dụng của bạn.

### 1. Đo lường SQL Queries (System.Data / Dapper / EF Core)
Bọc DbCommand của bạn bằng PerfDbCommand. Tham số nhạy cảm trong câu lệnh SQL sẽ tự động được Redaction.

`csharp
using Perf.Sql;
using Microsoft.Data.SqlClient;

using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

using var rawCommand = connection.CreateCommand();
rawCommand.CommandText = "SELECT * FROM Users WHERE Email = @email";

// Wrap nó lại bằng PerfDbCommand
using var perfCommand = new PerfDbCommand(rawCommand);
await perfCommand.ExecuteReaderAsync(); // Tự động ghi lại Span "SQL ExecuteReader"
`

### 2. Đo lường ngoại gọi HTTP (HttpClient)
Gắn PerfHttpMessageHandler vào HttpClient của bạn để nó tự động track mọi request gửi tới API bên thứ 3.

`csharp
using Perf.Net;

var client = new HttpClient(new PerfHttpMessageHandler(new SocketsHttpHandler()));

// Tự động sinh ra span "HTTP GET https://api.stripe.com"
await client.GetAsync("https://api.stripe.com/v1/customers");
`

---

## 📊 Xuất Báo Cáo (HTML, JSON, CSV)

Bạn muốn lưu lại vết Trace để phân tích trên Dashboard hoặc gửi cho bộ phận QA? 

`csharp
using Perf.Core.Exporters;

var trace = TraceBuilder.Build(listOfCollectedSpans);

// Xuất file HTML dạng Gantt Chart
var htmlExporter = new HtmlExporter("C:/Logs/Traces");
await htmlExporter.ExportAsync(trace);

// Xuất CSV
var csvExporter = new CsvExporter("C:/Logs/traces.csv");
await csvExporter.ExportAsync(trace);
`

Mở file 	race_XYZ.html lên bằng trình duyệt, bạn sẽ thấy giao diện **Flame Graph (Gantt Chart)** cực kỳ trực quan với màu sắc, thời gian milli-giây (ms), và dung lượng RAM cấp phát (Bytes) trên mỗi đầu mục!

---
*Dự án PerfFramework - Sẵn sàng cho Production ở những hệ thống khắt khe nhất.*
