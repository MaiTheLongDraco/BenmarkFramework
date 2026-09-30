using System;
using System.Threading;
using System.Threading.Tasks;

namespace Perf.Core;

public static class PerfLock
{
    private static readonly OperationId _semaphoreOpId = OperationRegistry.Register("Lock.SemaphoreSlim");

    public static async Task<IDisposable> MeasureWaitAsync(SemaphoreSlim semaphore, CancellationToken ct = default)
    {
        using (Perf.Measure(_semaphoreOpId))
        {
            await semaphore.WaitAsync(ct);
        }
        // Could return a releaser scope if we want to track execution time inside the lock too.
        return new Releaser(semaphore);
    }

    private readonly struct Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public Releaser(SemaphoreSlim semaphore) { _semaphore = semaphore; }
        public void Dispose() => _semaphore.Release();
    }
}