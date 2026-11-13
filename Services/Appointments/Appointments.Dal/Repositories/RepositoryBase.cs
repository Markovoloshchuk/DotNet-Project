using System.Data;
using Microsoft.Data.SqlClient;

namespace Appointments.Dal.Repositories;

public abstract class RepositoryBase
{
    protected readonly SqlConnection Connection;

    private readonly Func<SqlTransaction?> _currentTransaction;

    protected SqlTransaction? Transaction => _currentTransaction();

    protected RepositoryBase(
        SqlConnection connection,
        Func<SqlTransaction?> currentTransaction)
    {
        Connection = connection;
        _currentTransaction = currentTransaction;
    }

    protected async Task EnsureOpenAsync(CancellationToken ct)
    {
        if (Connection.State == ConnectionState.Broken)
        {
            await Connection.CloseAsync();
        }

        if (Connection.State != ConnectionState.Open)
        {
            await Connection.OpenAsync(ct);
        }
    }
}