using System.Collections.ObjectModel;
using System.Dynamic;

namespace Core.Services.Consumables.Operations
{
    public interface IOperationsDataGridService
    {
        Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName, int materialId);
        Task<int> InsertRecordAsync(string tableName, int materialId, string operationType,
            string dateTime, string quantity, string description, int user);
    }
}

