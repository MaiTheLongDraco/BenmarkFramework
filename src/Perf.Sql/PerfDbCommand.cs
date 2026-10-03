using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Perf.Core;

namespace Perf.Sql;

public class PerfDbCommand : DbCommand
{
    private readonly DbCommand _inner;
    private readonly OperationId _sqlOpId;

    public PerfDbCommand(DbCommand inner)
    {
        _inner = inner;
        _sqlOpId = OperationRegistry.Register("SQL.Execute");
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        using var scope = (PerfScope)Perf.Core.Perf.Measure(_sqlOpId);
        try
        {
            return await _inner.ExecuteReaderAsync(behavior, cancellationToken);
        }
        catch (Exception ex)
        {
            scope.MarkFailed(ex);
            throw;
        }
    }
    
    // Abstract overrides forwarding to _inner omitted for brevity in Phase 6 demo.
    // In a real adapter, all abstract properties and methods of DbCommand are forwarded.
    
#nullable disable
    public override string CommandText { get => _inner.CommandText; set => _inner.CommandText = value; }
    public override int CommandTimeout { get => _inner.CommandTimeout; set => _inner.CommandTimeout = value; }
    public override CommandType CommandType { get => _inner.CommandType; set => _inner.CommandType = value; }
    public override bool DesignTimeVisible { get => _inner.DesignTimeVisible; set => _inner.DesignTimeVisible = value; }
    public override UpdateRowSource UpdatedRowSource { get => _inner.UpdatedRowSource; set => _inner.UpdatedRowSource = value; }
    protected override DbConnection DbConnection { get => _inner.Connection; set => _inner.Connection = value; }
    protected override DbParameterCollection DbParameterCollection => _inner.Parameters;
    protected override DbTransaction DbTransaction { get => _inner.Transaction; set => _inner.Transaction = value; }
#nullable restore
    public override void Cancel() => _inner.Cancel();
    public override int ExecuteNonQuery() => _inner.ExecuteNonQuery();
    public override object? ExecuteScalar() => _inner.ExecuteScalar();
    public override void Prepare() => _inner.Prepare();
    protected override DbParameter CreateDbParameter() => _inner.CreateParameter();
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => _inner.ExecuteReader(behavior);
}