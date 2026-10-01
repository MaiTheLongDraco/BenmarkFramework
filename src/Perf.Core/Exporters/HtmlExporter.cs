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

        string htmlContent = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>PerfTrace: {trace.TraceId}</title>
    <style>
        :root {{
            --bg: #1e1e1e;
            --text: #d4d4d4;
            --border: #333;
            --bar-bg: #007acc;
            --bar-hover: #0098ff;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: var(--bg);
            color: var(--text);
            margin: 0;
            padding: 20px;
        }}
        .header {{ margin-bottom: 20px; border-bottom: 1px solid var(--border); padding-bottom: 10px; }}
        .gantt-container {{
            position: relative;
            border: 1px solid var(--border);
            overflow-x: auto;
            min-height: 400px;
        }}
        .row {{
            position: relative;
            height: 30px;
            border-bottom: 1px dashed #2a2a2a;
            display: flex;
            align-items: center;
        }}
        .row:hover {{ background-color: #2a2d2e; }}
        .label-col {{
            width: 300px;
            min-width: 300px;
            border-right: 1px solid var(--border);
            padding-left: 10px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            z-index: 10;
            background: var(--bg);
            font-size: 14px;
        }}
        .timeline-col {{
            flex-grow: 1;
            position: relative;
        }}
        .bar {{
            position: absolute;
            height: 20px;
            background-color: var(--bar-bg);
            border-radius: 3px;
            cursor: pointer;
            transition: background-color 0.2s;
            display: flex;
            align-items: center;
            padding: 0 5px;
            box-sizing: border-box;
            font-size: 11px;
            overflow: hidden;
            white-space: nowrap;
        }}
        .bar:hover {{ background-color: var(--bar-hover); }}
        .tooltip {{
            display: none;
            position: absolute;
            background: #252526;
            border: 1px solid #454545;
            padding: 10px;
            border-radius: 5px;
            z-index: 100;
            box-shadow: 0 4px 6px rgba(0,0,0,0.3);
            font-size: 13px;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <h2>Trace: {trace.TraceId}</h2>
    </div>
    
    <div class=""gantt-container"" id=""gantt""></div>
    <div class=""tooltip"" id=""tooltip""></div>

    <script>
        const traceData = {traceJson};
        
        let operationMeta = {{}};
        // Ideally we would dump the OperationRegistry here too, but for simplicity, we mock generic names.
        
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
            
            rowHtml += `
            <div class=""row"">
                <div class=""label-col"" style=""padding-left: ${{indent + 10}}px;"" title=""OpId: ${{span.OperationId.Value}}"">
                    Op ${{span.OperationId.Value}}
                </div>
                <div class=""timeline-col"">
                    <div class=""bar"" style=""left: ${{startPct}}%; width: ${{Math.max(widthPct, 0.1)}}%;"" 
                         onmouseover=""showTooltip(event, '${{span.SpanId.Value}}', '${{durMs}}', ${{span.AllocatedBytes}})""
                         onmouseout=""hideTooltip()"">
                         ${{durMs}}
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