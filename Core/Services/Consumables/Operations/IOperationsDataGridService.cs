using System.Collections.ObjectModel;
using System.Dynamic;
using Data.Repositories.Consumables.Operations;

namespace Core.Services.Consumables.Operations
{
    public interface IOperationsDataGridService
    {
        Task<List<OperationDto>> GetDataAsync(string tableName, int materialId);
        Task<int> InsertRecordAsync(string tableName, int materialId, string operationType,
            string dateTime, string quantity, string description, int user, byte[] receiptImageBytes);
    }
}

