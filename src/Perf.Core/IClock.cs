namespace Perf.Core;

public interface IClock
{
    long GetTimestamp();
    TimeSpan GetElapsedTime(long startTimestamp, long endTimestamp);
}

public sealed class StopwatchClock : IClock
{
    public static readonly StopwatchClock Instance = new();

    public long GetTimestamp() => System.Diagnostics.Stopwatch.GetTimestamp();

    public TimeSpan GetElapsedTime(long startTimestamp, long endTimestamp)
    {
        return TimeSpan.FromTicks((endTimestamp - startTimestamp) * TimeSpan.TicksPerSecond / System.Diagnostics.Stopwatch.Frequency);
    }
}