using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Appointments.Dal.Interfaces;

namespace Appointments.Dal.Repositories;

public abstract class BaseDapperRepository<T> : RepositoryBase, IGenericRepository<T>
    where T : class
{
    private readonly string _tableName;
    private readonly string _idColumn;
    private readonly string _idParameterName;

    private readonly string _insertSql;
    private readonly string _updateSql;
    private readonly string _deleteSql;
    private readonly string _getByIdSql;
    private readonly string _getAllSql;

    protected BaseDapperRepository(
        SqlConnection connection,
        Func<SqlTransaction?> currentTransaction,
        string tableName,
        string idColumn,
        IReadOnlyCollection<string> insertColumns,
        IReadOnlyCollection<string> updateColumns)
        : base(connection, currentTransaction)
    {
        _tableName = QuoteTableName(tableName);
        _idColumn = QuoteIdentifier(idColumn);
        _idParameterName = idColumn;

        var columns = insertColumns.ToArray();
        var updateFields = updateColumns.ToArray();

        if (columns.Length == 0 || updateFields.Length == 0)
            throw new ArgumentException("Insert and update columns cannot be empty.");

        var insertColumnList = string.Join(", ", columns.Select(QuoteIdentifier));
        var insertParameters = string.Join(", ", columns.Select(c => $"@{c}"));

        _insertSql = $"""
            INSERT INTO {_tableName} ({insertColumnList})
            OUTPUT INSERTED.{_idColumn}
            VALUES ({insertParameters});
            """;

        var updateSet = string.Join(
            ", ",
            updateFields.Select(c => $"{QuoteIdentifier(c)} = @{c}"));

        _updateSql = $"""
            UPDATE {_tableName}
            SET {updateSet}
            WHERE {_idColumn} = @{_idParameterName};
            """;

        _deleteSql = $"""
            DELETE FROM {_tableName}
            WHERE {_idColumn} = @{_idParameterName};
            """;

        _getByIdSql = $"""
            SELECT *
            FROM {_tableName}
            WHERE {_idColumn} = @{_idParameterName};
            """;

        _getAllSql = $"SELECT * FROM {_tableName};";
    }

    public virtual async Task<T?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        var parameters = new DynamicParameters();
        parameters.Add(_idParameterName, id);

        return await Connection.QuerySingleOrDefaultAsync<T>(
            new CommandDefinition(
                _getByIdSql,
                parameters,
                Transaction,
                cancellationToken: ct));
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        var result = await Connection.QueryAsync<T>(
            new CommandDefinition(
                _getAllSql,
                transaction: Transaction,
                cancellationToken: ct));

        return result.AsList();
    }

    public virtual async Task<long> AddAsync(
        T entity,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        return await Connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                _insertSql,
                entity,
                Transaction,
                cancellationToken: ct));
    }

    public virtual async Task<bool> UpdateAsync(
        T entity,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        var affectedRows = await Connection.ExecuteAsync(
            new CommandDefinition(
                _updateSql,
                entity,
                Transaction,
                cancellationToken: ct));

        return affectedRows > 0;
    }

    public virtual async Task<bool> DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        var parameters = new DynamicParameters();
        parameters.Add(_idParameterName, id);

        var affectedRows = await Connection.ExecuteAsync(
            new CommandDefinition(
                _deleteSql,
                parameters,
                Transaction,
                cancellationToken: ct));

        return affectedRows > 0;
    }

    private static string QuoteIdentifier(string identifier)
    {
        if (!Regex.IsMatch(identifier, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            throw new ArgumentException($"Invalid SQL identifier: {identifier}");

        return $"[{identifier}]";
    }

    private static string QuoteTableName(string tableName)
    {
        return string.Join(
            ".",
            tableName.Split('.').Select(QuoteIdentifier));
    }
}