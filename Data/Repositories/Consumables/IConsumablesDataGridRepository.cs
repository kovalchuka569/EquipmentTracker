using System.Collections.ObjectModel;
using System.Dynamic;

namespace Data.Repositories.Consumables
{
    public interface IConsumablesDataGridRepository
    {
        Task<IAsyncEnumerable<ExpandoObject>> GetDataAsync(string tableName);
        IAsyncEnumerable<string> StartListeningForChangesAsync(CancellationToken cancellationToken);
        Task<(int? min, int? max)> GetDataMinMaxAsync(string tableName, int recordId);
        Task UpdateMinLevelAsync(string tableName, int recordId, int? min);
        Task UpdateMaxLevelAsync(string tableName, int recordId, int? max);
        Task<int> DecreaseQuantityAsync(string tableName, int materialId, string quantity);
        Task<int> IncreaseQuantityAsync(string tableName, int materialId, string quantity);
    }
}
