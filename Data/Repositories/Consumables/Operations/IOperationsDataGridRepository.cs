using System.Collections.ObjectModel;
using System.Dynamic;

namespace Data.Repositories.Consumables.Operations
{
    public interface IOperationsDataGridRepository
    {
        Task<List<OperationDto>> LoadDataAsync(string tableName, int materialId);
        Task<int> InsertRecordAsync(string operationsTableName, string tableName, int materialId, string operationType,
         string dateTime, string quantity, string description, int user, byte[] receiptImageBytes);
    }
}
