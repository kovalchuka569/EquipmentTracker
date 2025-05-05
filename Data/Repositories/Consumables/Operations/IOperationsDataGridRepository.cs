using System.Collections.ObjectModel;
using System.Dynamic;

namespace Data.Repositories.Consumables.Operations
{
    public interface IOperationsDataGridRepository
    {
     Task <IAsyncEnumerable<ExpandoObject>> LoadDataAsync(string tableName, int materialId);
     Task<int> InsertRecordAsync(string tableName, int materialId, string operationType,
         string dateTime, string quantity, string description, int user);
    }
}
