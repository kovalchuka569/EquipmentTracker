using System.Collections.ObjectModel;
using System.Dynamic;

namespace Core.Services.DataGrid
{
    public interface IDataGridService
    {
        Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName);
        Task<object> InsertRecordAsync(string tableName, Dictionary<string, object> values);
        Task UpdateRecordAsync(string tableName, object id, Dictionary<string, object> values);
        Task DeleteRecordAsync(string tableName, object id);
    }
}
