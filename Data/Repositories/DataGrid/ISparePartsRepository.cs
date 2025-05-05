using System.Collections.ObjectModel;
using System.Dynamic;

namespace Data.Repositories.DataGrid
{
    public interface ISparePartsRepository
    {
        Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName, object equipmentId);
        Task<object> InsertRecordAsync(string tableName, int equipmentId, Dictionary<string, object> values);
        Task UpdateRecordAsync(string tableName, object id, Dictionary<string, object> values);
        Task DeleteRecordAsync(string tableName, object id);
        IAsyncEnumerable<string> StartListeningForChangesAsync(CancellationToken cancellationToken, string tableName);
    }
}
