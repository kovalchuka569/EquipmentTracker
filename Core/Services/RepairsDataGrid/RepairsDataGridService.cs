using System.Collections.ObjectModel;
using Common.Logging;
using Data.Repositories.Repairs;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;

namespace Core.Services.RepairsDataGrid;

public class RepairsDataGridService : IRepairsDataGridService
{
    private readonly IRepairsRepository _repository;
    private readonly IAppLogger<RepairsDataGridService> _logger;

    private string _equipmentTableName;
    
    public RepairsDataGridService(IRepairsRepository repository, IAppLogger<RepairsDataGridService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<ObservableCollection<RepairItem>> GetRepairItems(string tableName, string equipmentTableName)
    { 
        try
        {
            var repairsFromDb = await _repository.GetDataAsync(tableName, equipmentTableName);
            var repairItems = repairsFromDb.Select(r => new RepairItem
            {
                Id = r.Id,
                EquipmentId = r.EquipmentId,
                BreakDescription = r.BreakDescription,
                EquipmentInventoryNumber = r.EquipmentInventoryNumber,
                EquipmentBrand = r.EquipmentBrand,
                EquipmentModel = r.EquipmentModel,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Duration = r.Duration,
                Worker = r.Worker,
                Status = r.Status,
            });
            return new ObservableCollection<RepairItem>(repairItems);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error inserting repair items");
            throw;
        }
    }
}