using System;

namespace Perf.Core;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class PerfAttribute : Attribute
{
    public string? OperationName { get; }

    public PerfAttribute(string? operationName = null)
    {
        OperationName = operationName;
    }
}