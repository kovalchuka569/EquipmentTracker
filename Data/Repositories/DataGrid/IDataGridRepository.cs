using System.Collections.ObjectModel;
using System.Dynamic;

namespace Data.Repositories.DataGrid
{
    public interface IDataGridRepository
    {
        Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName);
        Task<object> InsertRecordAsync(string tableName, Dictionary<string, object> values);
        Task UpdateRecordAsync(string tableName, object id, Dictionary<string, object> values);
        Task DeleteRecordAsync(string tableName, object id);
        Task<Dictionary<string, string>> GetColumnTypesAsync(string tableName);
    }
}
