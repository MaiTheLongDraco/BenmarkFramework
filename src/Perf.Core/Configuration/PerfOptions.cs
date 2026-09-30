namespace Perf.Core.Configuration;

public class PerfOptions
{
    public bool Enabled { get; set; } = true;
    public bool CaptureAllocations { get; set; } = true;
    public bool CaptureCpuTime { get; set; } = false;
    
    // Sampling rate: 1.0 means 100%, 0.0 means 0%
    public double SampleRate { get; set; } = 1.0;
    
    // If a request is slower than this, we might want to capture it regardless of SampleRate (requires buffered capture)
    public double SlowRequestThresholdMs { get; set; } = 5000.0;
}