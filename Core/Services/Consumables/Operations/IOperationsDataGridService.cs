using System.Collections.ObjectModel;
using System.Dynamic;
using Data.Repositories.Consumables.Operations;

namespace Core.Services.Consumables.Operations
{
    public interface IOperationsDataGridService
    {
        Task<List<OperationDto>> GetDataAsync(string operationsTableName, int materialId);
        Task<int> InsertRecordAsync(string operationsTableName, string tableName, int materialId, string operationType,
            string dateTime, string quantity, string description, int user, byte[] receiptImageBytes);
    }
}

