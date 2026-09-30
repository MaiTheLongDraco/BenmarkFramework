namespace Perf.Core;

public enum WaitReason : byte
{
    None = 0,
    Sql = 1,
    Http = 2,
    Redis = 3,
    FileIO = 4,
    Network = 5,
    Lock = 6,
    Semaphore = 7,
    Channel = 8,
    Task = 9,
    ThreadPool = 10,
    UnityAsyncOperation = 11,
    Addressables = 12,
    AssetBundle = 13,
    Unknown = 255
}