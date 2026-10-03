using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Perf.Core.Tracing;

namespace Perf.Core.Exporters;

public class HtmlExporter : IPerfExporter
{
    private readonly string _outputDirectory;

    public HtmlExporter(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
        Directory.CreateDirectory(_outputDirectory);
    }

    public async Task ExportAsync(PerfTrace trace)
    {
        string filePath = Path.Combine(_outputDirectory, $"trace_{trace.TraceId}.html");
        
        string traceJson = JsonSerializer.Serialize(trace, PerfTraceJsonContext.Default.PerfTrace);
        var metadataList = OperationRegistry.GetAllMetadata();
        string metaJson = JsonSerializer.Serialize(metadataList);

        string htmlContent = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>PerfTrace: {trace.TraceId}</title>
    <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap"" rel=""stylesheet"">
    <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
    <style>
        :root {{
            --bg: #0d1117;
            --surface: rgba(255, 255, 255, 0.03);
            --surface-hover: rgba(255, 255, 255, 0.08);
            --border: rgba(255, 255, 255, 0.1);
            --text-primary: #e6edf3;
            --text-secondary: #848d97;
            --bar-bg: linear-gradient(90deg, #3b82f6, #8b5cf6);
            --bar-glow: rgba(59, 130, 246, 0.4);
            --tab-active: #3b82f6;
        }}
        
        body {{
            font-family: 'Inter', sans-serif;
            background-color: var(--bg);
            background-image: radial-gradient(circle at top right, rgba(59, 130, 246, 0.1), transparent 40%),
                              radial-gradient(circle at bottom left, rgba(139, 92, 246, 0.1), transparent 40%);
            color: var(--text-primary);
            margin: 0;
            padding: 40px 20px;
            min-height: 100vh;
        }}
        
        .header {{ 
            margin-bottom: 20px;
            text-align: center;
        }}
        
        .header h2 {{
            font-weight: 600;
            font-size: 24px;
            margin: 0;
            background: -webkit-linear-gradient(#fff, #a5b4fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }}
        
        /* Tabs */
        .tabs {{
            display: flex;
            justify-content: center;
            margin-bottom: 20px;
            gap: 10px;
        }}
        .tab-btn {{
            background: var(--surface);
            border: 1px solid var(--border);
            color: var(--text-primary);
            padding: 8px 16px;
            border-radius: 6px;
            cursor: pointer;
            font-family: 'Inter', sans-serif;
            font-weight: 500;
            transition: all 0.2s;
        }}
        .tab-btn:hover {{
            background: var(--surface-hover);
        }}
        .tab-btn.active {{
            background: var(--tab-active);
            border-color: var(--tab-active);
            box-shadow: 0 0 10px var(--bar-glow);
        }}
        
        .tab-content {{
            display: none;
            background: var(--surface);
            backdrop-filter: blur(12px);
            -webkit-backdrop-filter: blur(12px);
            border: 1px solid var(--border);
            border-radius: 12px;
            min-height: 400px;
            max-width: 1400px;
            margin: 0 auto;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
            padding: 20px;
        }}
        .tab-content.active {{
            display: block;
        }}
        
        /* Gantt */
        .gantt-container {{
            position: relative;
            overflow-x: auto;
            overflow-y: auto;
            max-height: 600px;
        }}
        .row {{
            position: relative;
            height: 44px;
            border-bottom: 1px solid var(--border);
            display: flex;
            align-items: center;
            transition: background-color 0.2s ease;
        }}
        .row:last-child {{ border-bottom: none; }}
        .row:hover {{ background-color: var(--surface-hover); }}
        
        .label-col {{
            width: 320px;
            min-width: 320px;
            border-right: 1px solid var(--border);
            padding-left: 20px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            z-index: 10;
            font-size: 13px;
            font-weight: 500;
            display: flex;
            align-items: center;
        }}
        .timeline-col {{
            flex-grow: 1;
            position: relative;
            height: 100%;
        }}
        .bar {{
            position: absolute;
            height: 24px;
            top: 10px;
            background: var(--bar-bg);
            border-radius: 6px;
            cursor: pointer;
            display: flex;
            align-items: center;
            padding: 0 10px;
            box-sizing: border-box;
            font-size: 11px;
            font-weight: 600;
            color: #fff;
            overflow: hidden;
            white-space: nowrap;
            box-shadow: 0 4px 12px var(--bar-glow);
            transition: transform 0.2s cubic-bezier(0.4, 0, 0.2, 1), filter 0.2s;
        }}
        .bar:hover {{ 
            transform: scaleY(1.1);
            filter: brightness(1.2);
        }}
        
        /* Hierarchy Table */
        .table-container {{ overflow-x: auto; max-height: 600px; }}
        table {{ width: 100%; border-collapse: collapse; font-size: 13px; text-align: left; }}
        th, td {{ padding: 10px 15px; border-bottom: 1px solid var(--border); }}
        th {{ background: rgba(255,255,255,0.05); font-weight: 600; color: #fff; position: sticky; top: 0; z-index: 2; }}
        tr:hover td {{ background: var(--surface-hover); }}
        
        /* Charts */
        .charts-container {{
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            justify-content: center;
        }}
        .chart-box {{
            width: 45%;
            min-width: 400px;
            background: rgba(0,0,0,0.2);
            padding: 15px;
            border-radius: 8px;
            border: 1px solid var(--border);
        }}
        
        .tooltip {{
            display: none;
            position: absolute;
            background: rgba(15, 23, 42, 0.9);
            backdrop-filter: blur(8px);
            border: 1px solid rgba(255, 255, 255, 0.15);
            padding: 16px;
            border-radius: 8px;
            z-index: 100;
            box-shadow: 0 10px 25px -5px rgba(0,0,0,0.5);
            font-size: 13px;
            color: #cbd5e1;
            line-height: 1.5;
            pointer-events: none;
        }}
        .tooltip strong {{ color: #fff; font-weight: 600; }}
        
        ::-webkit-scrollbar {{ width: 10px; height: 10px; }}
        ::-webkit-scrollbar-track {{ background: transparent; }}
        ::-webkit-scrollbar-thumb {{ background: #334155; border-radius: 5px; }}
        ::-webkit-scrollbar-thumb:hover {{ background: #475569; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h2>Performance Trace Analysis</h2>
        <div style=""color: var(--text-secondary); font-size: 14px; margin-top: 8px;"">ID: {trace.TraceId}</div>
    </div>
    
    <div class=""tabs"">
        <button class=""tab-btn active"" onclick=""openTab('tab-gantt', this)"">Timeline</button>
        <button class=""tab-btn"" onclick=""openTab('tab-hierarchy', this)"">Hierarchy Statistics</button>
        <button class=""tab-btn"" onclick=""openTab('tab-analytics', this)"">Analytics & Charts</button>
    </div>
    
    <!-- Timeline Tab -->
    <div id=""tab-gantt"" class=""tab-content active"">
        <div class=""gantt-container"" id=""gantt""></div>
    </div>
    
    <!-- Hierarchy Tab -->
    <div id=""tab-hierarchy"" class=""tab-content"">
        <div class=""table-container"">
            <table>
                <thead>
                    <tr>
                        <th>Operation Name</th>
                        <th>Calls</th>
                        <th>Total Time (ms)</th>
                        <th>Self Time (ms)</th>
                        <th>Total GC Alloc (B)</th>
                        <th>Self GC Alloc (B)</th>
                    </tr>
                </thead>
                <tbody id=""hierarchy-tbody""></tbody>
            </table>
        </div>
    </div>
    
    <!-- Analytics Tab -->
    <div id=""tab-analytics"" class=""tab-content"">
        <div class=""charts-container"">
            <div class=""chart-box"">
                <canvas id=""chart-time""></canvas>
            </div>
            <div class=""chart-box"">
                <canvas id=""chart-alloc""></canvas>
            </div>
            <div class=""chart-box"" style=""width: 90%; margin-top: 20px;"">
                <canvas id=""chart-line-time""></canvas>
            </div>
        </div>
    </div>
    
    <div class=""tooltip"" id=""tooltip""></div>

    <script>
        const traceData = {traceJson};
        
        // Build metadata lookup object
        const operationMeta = {{}};
        const metaList = {metaJson};
        for (const meta of metaList) {{
            operationMeta[meta.Id.Value] = meta.Name;
        }}
        
        // Tab switching
        function openTab(tabId, btn) {{
            document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
            document.querySelectorAll('.tab-btn').forEach(el => el.classList.remove('active'));
            document.getElementById(tabId).classList.add('active');
            btn.classList.add('active');
        }}
        
        const ganttContainer = document.getElementById('gantt');
        const tooltip = document.getElementById('tooltip');
        
        function parseTimeSpanToMs(timeStr) {{
            if (!timeStr) return 0;
            let days = 0;
            let timePart = timeStr;
            if (timeStr.indexOf('.') !== -1 && timeStr.indexOf(':') !== -1) {{
                const firstDot = timeStr.indexOf('.');
                const firstColon = timeStr.indexOf(':');
                if (firstDot < firstColon) {{
                    const parts = timeStr.split('.');
                    days = parseInt(parts[0]) || 0;
                    timePart = parts.slice(1).join('.');
                }}
            }}
            const parts = timePart.split(':');
            if (parts.length === 3) {{
                let h = parseInt(parts[0], 10) || 0;
                let m = parseInt(parts[1], 10) || 0;
                let s = parseFloat(parts[2]) || 0;
                return (days * 86400 + h * 3600 + m * 60 + s) * 1000;
            }}
            return 0;
        }}
        
        if (!traceData.Root) {{
            ganttContainer.innerHTML = 'No data available or empty trace.';
        }} else {{
            // --- 1. Gantt Chart Logic ---
            let minTs = traceData.Root.Span.StartTimestamp;
            let maxTs = traceData.Root.Span.EndTimestamp;
            
            function findExtremes(node) {{
                if (node.Span.StartTimestamp < minTs) minTs = node.Span.StartTimestamp;
                if (node.Span.EndTimestamp > maxTs) maxTs = node.Span.EndTimestamp;
                node.Children.forEach(c => findExtremes(c));
            }}
            findExtremes(traceData.Root);
            
            const totalDuration = maxTs - minTs;
            let rowHtml = '';
            
            // --- 2. Aggregation Logic for Stats ---
            const stats = {{}};
            const timeSeries = []; // array of {{ ts, duration, alloc, opName }}
            
            function drawAndAggregate(node, depth) {{
                const span = node.Span;
                const opId = span.OperationId.Value;
                const opName = operationMeta[opId] || ('Op ' + opId);
                const durMs = parseTimeSpanToMs(span.Duration);
                
                // Aggregation
                if (!stats[opId]) {{
                    stats[opId] = {{ name: opName, calls: 0, totalMs: 0, selfMs: 0, totalAlloc: 0, selfAlloc: 0 }};
                }}
                
                let childMs = 0;
                let childAlloc = 0;
                node.Children.forEach(c => {{
                    childMs += parseTimeSpanToMs(c.Span.Duration);
                    childAlloc += c.Span.AllocatedBytes;
                }});
                
                let selfMs = durMs - childMs;
                if (selfMs < 0) selfMs = 0;
                
                let selfAlloc = span.AllocatedBytes - childAlloc;
                if (selfAlloc < 0) selfAlloc = 0;
                
                stats[opId].calls++;
                stats[opId].totalMs += durMs;
                stats[opId].selfMs += selfMs;
                stats[opId].totalAlloc += span.AllocatedBytes;
                stats[opId].selfAlloc += selfAlloc;
                
                timeSeries.push({{ ts: span.StartTimestamp - minTs, duration: durMs, opName: opName }});
                
                // Gantt Drawing
                const startPct = ((span.StartTimestamp - minTs) / totalDuration) * 100;
                const widthPct = ((span.EndTimestamp - span.StartTimestamp) / totalDuration) * 100;
                const indent = depth * 20;
                
                rowHtml += `
                <div class=""row"">
                    <div class=""label-col"" style=""padding-left: ${{indent + 10}}px;"" title=""${{opName}}"">
                        ${{opName}}
                    </div>
                    <div class=""timeline-col"">
                        <div class=""bar"" style=""left: ${{startPct}}%; width: ${{Math.max(widthPct, 0.1)}}%;"" 
                             onmouseover=""showTooltip(event, '${{span.SpanId.Value}}', '${{durMs.toFixed(3)}} ms', ${{span.AllocatedBytes}})""
                             onmouseout=""hideTooltip()"">
                             ${{durMs.toFixed(2)}}ms | ${{span.AllocatedBytes}} B
                        </div>
                    </div>
                </div>`;
                
                node.Children.forEach(c => drawAndAggregate(c, depth + 1));
            }}
            
            drawAndAggregate(traceData.Root, 0);
            
            if(totalDuration === 0) {{ 
                ganttContainer.innerHTML = '<div style=""padding: 10px;"">Duration is zero. Only 1 point in time.</div>' + rowHtml; 
            }} else {{
                ganttContainer.innerHTML = rowHtml;
            }}
            
            // --- 3. Populate Hierarchy Table ---
            const tbody = document.getElementById('hierarchy-tbody');
            const sortedStats = Object.values(stats).sort((a,b) => b.totalMs - a.totalMs);
            
            sortedStats.forEach(s => {{
                tbody.innerHTML += `
                    <tr>
                        <td><strong>${{s.name}}</strong></td>
                        <td>${{s.calls}}</td>
                        <td>${{s.totalMs}}</td>
                        <td>${{s.selfMs}}</td>
                        <td>${{s.totalAlloc}}</td>
                        <td>${{s.selfAlloc}}</td>
                    </tr>
                `;
            }});
            
            // --- 4. Render Charts using Chart.js ---
            const labels = sortedStats.map(s => s.name);
            const dataTime = sortedStats.map(s => s.selfMs);
            const dataAlloc = sortedStats.map(s => s.selfAlloc);
            
            // Doughnut: Time Distribution
            new Chart(document.getElementById('chart-time'), {{
                type: 'doughnut',
                data: {{
                    labels: labels,
                    datasets: [{{
                        label: 'Self Time (ms)',
                        data: dataTime,
                        backgroundColor: ['#3b82f6', '#8b5cf6', '#ec4899', '#f43f5e', '#f59e0b', '#10b981'],
                        borderWidth: 0
                    }}]
                }},
                options: {{ plugins: {{ title: {{ display: true, text: 'Self Time Distribution (ms)', color: '#fff' }}, legend: {{ labels: {{ color: '#fff' }} }} }} }}
            }});
            
            // Bar: Alloc Distribution
            new Chart(document.getElementById('chart-alloc'), {{
                type: 'bar',
                data: {{
                    labels: labels,
                    datasets: [{{
                        label: 'Self GC Alloc (B)',
                        data: dataAlloc,
                        backgroundColor: '#8b5cf6',
                        borderRadius: 4
                    }}]
                }},
                options: {{ 
                    plugins: {{ title: {{ display: true, text: 'Self GC Allocations (Bytes)', color: '#fff' }}, legend: {{ labels: {{ color: '#fff' }} }} }},
                    scales: {{ x: {{ ticks: {{ color: '#848d97' }} }}, y: {{ ticks: {{ color: '#848d97' }} }} }}
                }}
            }});
            
            // Line: Time over execution (scatter-like or line)
            timeSeries.sort((a,b) => a.ts - b.ts);
            new Chart(document.getElementById('chart-line-time'), {{
                type: 'line',
                data: {{
                    labels: timeSeries.map((t, i) => i), // Exec order
                    datasets: [{{
                        label: 'Operation Duration (ms)',
                        data: timeSeries.map(t => t.duration),
                        borderColor: '#3b82f6',
                        tension: 0.3,
                        pointBackgroundColor: '#ec4899',
                        pointRadius: 3
                    }}]
                }},
                options: {{
                    responsive: true,
                    plugins: {{ 
                        title: {{ display: true, text: 'Execution Duration Over Time', color: '#fff' }}, 
                        legend: {{ labels: {{ color: '#fff' }} }},
                        tooltip: {{
                            callbacks: {{
                                label: function(ctx) {{
                                    const tsObj = timeSeries[ctx.dataIndex];
                                    return tsObj.opName + ': ' + tsObj.duration + ' ms';
                                }}
                            }}
                        }}
                    }},
                    scales: {{ x: {{ display: false }}, y: {{ ticks: {{ color: '#848d97' }} }} }}
                }}
            }});
        }}
        
        window.showTooltip = function(e, spanId, dur, alloc) {{
            tooltip.style.display = 'block';
            tooltip.style.left = e.pageX + 15 + 'px';
            tooltip.style.top = e.pageY + 15 + 'px';
            tooltip.innerHTML = `<strong>Span:</strong> ${{spanId}}<br><strong>Duration:</strong> ${{dur}}<br><strong>Allocated:</strong> ${{alloc}} bytes`;
        }};
        window.hideTooltip = function() {{ tooltip.style.display = 'none'; }};
    </script>
</body>
</html>";

        await File.WriteAllTextAsync(filePath, htmlContent);
    }
}