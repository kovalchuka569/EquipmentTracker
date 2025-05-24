using System.Data.Common;

namespace Data;

public static class DbReaderExtensions
{
    public static T GetValueOrDefault<T>(this DbDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
        {
            return default(T);
        }
        else
        {
            return (T)reader.GetValue(ordinal);
        }
    }
}