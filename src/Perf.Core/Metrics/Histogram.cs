using System;
using System.Linq;

namespace Perf.Core.Metrics;

/// <summary>
/// A simple streaming histogram using buckets.
/// Suitable for low memory overhead percentile estimation.
/// </summary>
public class Histogram
{
    // Buckets from 0 to 10s roughly, exponential-ish distribution
    // For a real production app, HDR Histogram is recommended, but this is a lightweight approach.
    private readonly double[] _bounds;
    private readonly long[] _counts;
    
    public long TotalCount { get; private set; }
    public double Sum { get; private set; }
    public double Min { get; private set; } = double.MaxValue;
    public double Max { get; private set; } = double.MinValue;

    public Histogram()
    {
        _bounds = new double[] 
        { 
            0.1, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, double.MaxValue 
        };
        _counts = new long[_bounds.Length];
    }

    public void Record(double value)
    {
        lock (_counts) // Thread-safe aggregation
        {
            TotalCount++;
            Sum += value;
            if (value < Min) Min = value;
            if (value > Max) Max = value;

            for (int i = 0; i < _bounds.Length; i++)
            {
                if (value <= _bounds[i])
                {
                    _counts[i]++;
                    break;
                }
            }
        }
    }

    public double GetPercentile(double percentile)
    {
        lock (_counts)
        {
            if (TotalCount == 0) return 0;
            if (TotalCount == 1) return Max;
            if (percentile <= 0) return Min;
            if (percentile >= 100) return Max;

            double targetCount = TotalCount * (percentile / 100.0);
            long cumulativeCount = 0;

            for (int i = 0; i < _bounds.Length; i++)
            {
                cumulativeCount += _counts[i];
                if (cumulativeCount >= targetCount)
                {
                    // Linear interpolation within bucket
                    double bucketMin = i == 0 ? 0 : _bounds[i - 1];
                    double bucketMax = _bounds[i] == double.MaxValue ? Max : _bounds[i];
                    
                    long countInBucket = _counts[i];
                    long previousCumulative = cumulativeCount - countInBucket;
                    
                    double fraction = (targetCount - previousCumulative) / (double)countInBucket;
                    return bucketMin + (bucketMax - bucketMin) * fraction;
                }
            }
            return Max;
        }
    }

    public PerfStatistics GetStatistics()
    {
        lock (_counts)
        {
            if (TotalCount == 0) return new PerfStatistics();
            return new PerfStatistics(
                TotalCount,
                Min,
                Max,
                Sum / TotalCount,
                GetPercentile(50),
                GetPercentile(90),
                GetPercentile(95),
                GetPercentile(99)
            );
        }
    }
}