using System.Collections.ObjectModel;
using System.Dynamic;
using Data.Repositories.Consumables;
using Models.ConsumablesDataGrid;

namespace Core.Services.Consumables
{
    public interface IConsumablesDataGridService
    {
        Task<ObservableCollection<ConsumableItem>> GetDataAsync(string tableName);
        Task StartListeningForChangesAsync(CancellationToken cancellationToken);
        Task<(int? min, int? max)> GetDataMinMaxAsync(string tableName, int recordId);
        Task UpdateMinLevelAsync(string tableName, int recordId, int? min);
        Task UpdateMaxLevelAsync(string tableName, int recordId, int? max);
        Task<int> UpdateQuantityAsync(string tableName, int materialId, string quantity, string operation);
    }
}
