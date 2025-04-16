using System.Collections.ObjectModel;
using System.Dynamic;

namespace Core.Services.DataGrid
{
    public interface ISparePartsService
    {
        Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName, object equipmentId);
    }
}
