using System.Reflection;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;
using DotnetExam.Data.Contexts;
using DotnetExam.Data.Models.Common;

namespace DotnetExam.Data.Services;

public class ClickHouseRepository<TEntity>(AppClickHouseContext context) : IClickHouseRepository<TEntity>
    where TEntity : EntityBase
{
    public async Task AddAsync(TEntity entity)
    {
        var tableName = GetTableName();

        if (await ExistsAsync(entity))
            throw new Exception($"Entity with Id={entity.Id} already exists in {tableName}");

        var props = typeof(TEntity).GetProperties();
        var columns = string.Join(", ", props.Select(p => p.Name));

        var parameters = new Dictionary<string, string?>();

        var values = props.Select(p =>
        {
            parameters[p.Name] = FormatValue(p.GetValue(entity));
            return GetParameterPlaceholder(p);
        });
        var valuesJoin = $"({string.Join(", ", values)})";

        var sql = $"INSERT INTO {GetTableName()} ({columns}) VALUES \n{valuesJoin}";

        await ExecuteNonQueryAsync(sql, parameters);
    }

    public async Task UpdateAsync(TEntity entity)
    {
        var tableName = GetTableName();

        if (!await ExistsAsync(entity))
            throw new InvalidOperationException($"Entity with Id={entity.Id} does not exist in {tableName}");

        var props = typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.Name != "Id")
            .ToList();

        var parameters = new Dictionary<string, string?> { { "Id", FormatValue(entity.Id) } };

        var setClauses = props.Select(p =>
        {
            parameters[p.Name] = FormatValue(p.GetValue(entity));
            return $"{p.Name} = {GetParameterPlaceholder(p)}";
        });

        var setClauseString = string.Join(", ", setClauses);
        var updateSql = $"ALTER TABLE {tableName} UPDATE {setClauseString} WHERE {GetWhereByIdClause()}";

        await ExecuteNonQueryAsync(updateSql, parameters);
    }

    public async Task RemoveAsync(int id)
    {
        var sql = $"ALTER TABLE {GetTableName()} DELETE WHERE Id = @id"; // param

        await ExecuteNonQueryAsync(sql, new Dictionary<string, string?>() { { "id", id.ToString() } });
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var sql = $"SELECT * FROM {GetTableName()}";

        return await ExecuteQueryAsync(sql, null);
    }

    private async Task ExecuteNonQueryAsync(string sql, IDictionary<string, string?>? parameters)
    {
        await using var command = await GetCommand();

        command.CommandText = sql;
        if (parameters is not null)
            foreach (var (key, value) in parameters)
            {
                command.AddParameter(key, value);
            }

        await command.ExecuteNonQueryAsync();
    }

    private async Task<List<TEntity>> ExecuteQueryAsync(string sql, IDictionary<string, string?>? parameters)
    {
        await using var command = await GetCommand();

        command.CommandText = sql;
        if (parameters is not null)
            foreach (var (key, value) in parameters)
            {
                command.AddParameter(key, value);
            }

        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<TEntity>();
        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        while (await reader.ReadAsync())
        {
            var instance = Activator.CreateInstance<TEntity>();

            foreach (var prop in properties)
            {
                try
                {
                    var ordinal = reader.GetOrdinal(prop.Name);
                    if (reader.IsDBNull(ordinal)) continue;

                    var value = reader.GetValue(ordinal);
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    var convertedValue = Convert.ChangeType(value, targetType);
                    prop.SetValue(instance, convertedValue);
                }
                catch
                {
                    // ignored
                }
            }

            result.Add(instance);
        }

        return result;
    }

    private async Task<bool> ExistsAsync(TEntity entity)
    {
        var whereClause = GetWhereByIdClause();

        var sql = $"SELECT count() FROM {GetTableName()} WHERE {whereClause}";

        await using var command = await GetCommand();

        command.CommandText = sql;
        command.AddParameter("Id", entity.Id);

        var result = await command.ExecuteScalarAsync();
        if (result is ulong count)
            return count > 0;

        return false;
    }

    private static string? FormatValue(object? value)
    {
        return value switch
        {
            null => "NULL",
            string s => $"'{s.Replace("'", "''")}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            bool b => b ? "1" : "0",
            _ => value.ToString()
        };
    }

    private static string MapToClickHouseType(Type type)
    {
        if (type == typeof(int) || type == typeof(uint)) return "UInt32";
        if (type == typeof(long) || type == typeof(ulong)) return "UInt64";
        if (type == typeof(string)) return "String";
        if (type == typeof(DateTime)) return "DateTime";
        if (type == typeof(bool)) return "UInt8";
        if (type == typeof(float)) return "Float32";
        if (type == typeof(double)) return "Float64";

        throw new NotSupportedException($"Тип {type.Name} не поддерживается");
    }

    private static string GetTableName() => typeof(TEntity).Name ?? throw new Exception("Wrong type");

    private static string GetWhereByIdClause() => "Id = " + GetParameterPlaceholder(typeof(TEntity).GetProperty("Id")!);

    private static string GetParameterPlaceholder(PropertyInfo p) =>
        '{' + $"{p.Name}:{MapToClickHouseType(p.PropertyType)}" + '}';

    private async Task<ClickHouseCommand> GetCommand()
    {
        await using var connection = context.CreateConnection();
        await connection.OpenAsync();

        return connection.CreateCommand();
    }
}