using System.Windows.Controls;

namespace Data.Repositories.Common.DataGridColumns
{
    public interface IDataGridColumnRepository
    {
        Task<Dictionary<string, string>> GetColumnTypesAsync(string schema, string tableName);
    }
}

