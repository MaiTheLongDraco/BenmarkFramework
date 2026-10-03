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
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>PerfTrace: {trace.TraceId}</title>
    <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap"" rel=""stylesheet"">
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
            margin-bottom: 30px;
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
        
        .gantt-container {{
            position: relative;
            background: var(--surface);
            backdrop-filter: blur(12px);
            -webkit-backdrop-filter: blur(12px);
            border: 1px solid var(--border);
            border-radius: 12px;
            overflow-x: auto;
            overflow-y: hidden;
            min-height: 400px;
            max-width: 1400px;
            margin: 0 auto;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
        }}
        
        .row {{
            position: relative;
            height: 44px;
            border-bottom: 1px solid var(--border);
            display: flex;
            align-items: center;
            transition: background-color 0.2s ease;
        }}
        
        .row:last-child {{
            border-bottom: none;
        }}
        
        .row:hover {{ 
            background-color: var(--surface-hover); 
        }}
        
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
            color: var(--text-primary);
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
        
        .tooltip strong {{
            color: #fff;
            font-weight: 600;
        }}
        
        ::-webkit-scrollbar {{
            width: 10px;
            height: 10px;
        }}
        ::-webkit-scrollbar-track {{
            background: var(--bg);
        }}
        ::-webkit-scrollbar-thumb {{
            background: #334155;
            border-radius: 5px;
        }}
        ::-webkit-scrollbar-thumb:hover {{
            background: #475569;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <h2>Performance Trace Analysis</h2>
        <div style=""color: var(--text-secondary); font-size: 14px; margin-top: 8px;"">ID: {trace.TraceId}</div>
    </div>
    
    <div class=""gantt-container"" id=""gantt""></div>
    <div class=""tooltip"" id=""tooltip""></div>

    <script>
        const traceData = {traceJson};
        
        // Build metadata lookup object
        const operationMeta = {{}};
        const metaList = {metaJson};
        for (const meta of metaList) {{
            operationMeta[meta.Id.Value] = meta.Name;
        }}
        
        
        const container = document.getElementById('gantt');
        const tooltip = document.getElementById('tooltip');
        
        if (!traceData.Root) {{
            container.innerHTML = 'No data available or empty trace.';
        }} else {{
            // Find absolute min and max timestamps to scale
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
        
        function drawNode(node, depth) {{
            const span = node.Span;
            const startPct = ((span.StartTimestamp - minTs) / totalDuration) * 100;
            const widthPct = ((span.EndTimestamp - span.StartTimestamp) / totalDuration) * 100;
            
            // Format duration string
            const durMs = span.Duration;
            
            const indent = depth * 20;
            
            const opName = operationMeta[span.OperationId.Value] || ('Op ' + span.OperationId.Value);
            
            rowHtml += `
            <div class=""row"">
                <div class=""label-col"" style=""padding-left: ${{indent + 10}}px;"" title=""${{opName}}"">
                    ${{opName}}
                </div>
                <div class=""timeline-col"">
                    <div class=""bar"" style=""left: ${{startPct}}%; width: ${{Math.max(widthPct, 0.1)}}%;"" 
                         onmouseover=""showTooltip(event, '${{span.SpanId.Value}}', '${{durMs}}', ${{span.AllocatedBytes}})""
                         onmouseout=""hideTooltip()"">
                         ${{durMs}} | ${{span.AllocatedBytes}} B
                    </div>
                </div>
            </div>`;
            
            node.Children.forEach(c => drawNode(c, depth + 1));
        }}
        
            drawNode(traceData.Root, 0);
            if(totalDuration === 0) {{ 
                container.innerHTML = '<div style=""padding: 10px;"">Duration is zero. Only 1 point in time.</div>' + rowHtml; 
            }} else {{
                container.innerHTML = rowHtml;
            }}
        }}
        
        window.showTooltip = function(e, spanId, dur, alloc) {{
            tooltip.style.display = 'block';
            tooltip.style.left = e.pageX + 15 + 'px';
            tooltip.style.top = e.pageY + 15 + 'px';
            tooltip.innerHTML = `<strong>Span:</strong> ${{spanId}}<br><strong>Duration:</strong> ${{dur}}<br><strong>Allocated:</strong> ${{alloc}} bytes`;
        }};
        
        window.hideTooltip = function() {{
            tooltip.style.display = 'none';
        }};
    </script>
</body>
</html>";

        await File.WriteAllTextAsync(filePath, htmlContent);
    }
}