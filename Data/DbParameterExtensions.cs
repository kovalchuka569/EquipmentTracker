using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

public static class DbParameterExtensions
{
    // Для значень-структур (наприклад, int?, TimeSpan?)
    public static void AddWithNullableValue<T>(
        this DbParameterCollection parameters,
        string name,
        T? value,
        NpgsqlDbType npgsqlType
    ) where T : struct
    {
        var parameter = new NpgsqlParameter(name, npgsqlType)
        {
            Value = value.HasValue ? (object)value.Value : DBNull.Value
        };
        parameters.Add(parameter);
    }

    // Для класів (наприклад, string, byte[])
    public static void AddWithNullableValue<T>(
        this DbParameterCollection parameters,
        string name,
        T value,
        NpgsqlDbType npgsqlType
    ) where T : class
    {
        var parameter = new NpgsqlParameter(name, npgsqlType)
        {
            Value = value != null ? (object)value : DBNull.Value
        };
        parameters.Add(parameter);
    }
}