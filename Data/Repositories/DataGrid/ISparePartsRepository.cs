using System.Collections.ObjectModel;
using System.Dynamic;

namespace Data.Repositories.DataGrid
{
    public interface ISparePartsRepository
    {
        Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName, object equipmentId);
        Task<Dictionary<string, string>> GetColumnTypesAsync(string tableName);
    }
}
