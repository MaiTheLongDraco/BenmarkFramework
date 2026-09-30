using System.Threading.Channels;

namespace Perf.Core.Runtime;

public sealed class AsyncSpanRecorder : ISpanRecorder, IDisposable
{
    private readonly Channel<PerfSpan> _channel;

    public AsyncSpanRecorder(int capacity = 10000)
    {
        _channel = Channel.CreateBounded<PerfSpan>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest, // Prevent OOM, drop oldest traces if backend is slow
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void Record(PerfSpan span)
    {
        _channel.Writer.TryWrite(span);
    }

    public ChannelReader<PerfSpan> Reader => _channel.Reader;

    public void Dispose()
    {
        _channel.Writer.TryComplete();
    }
}