using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.Grid;

namespace Core.Services.Common.DataGridColumns
{
    public interface IDataGridColumnsService
    {
        Task<Dictionary<string, string>> GetColumnTypesAsync(string schema, string tableName);
        GridColumn CreateColumnFromDbType(string columnName, string dbType);
        public void SetBalanceLevels(int? maxLevel, int? minLevel, int? currentLevel);
    }
}
