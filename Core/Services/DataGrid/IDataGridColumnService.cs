using Syncfusion.UI.Xaml.Grid;

namespace Core.Services.DataGrid
{
    public interface IDataGridColumnService
    {
        GridColumn CreateColumnFromDbType(string columnName, string dbType);
        Task<Dictionary<string, string>> GetColumnTypesAsync(string tableName);
    }
}
