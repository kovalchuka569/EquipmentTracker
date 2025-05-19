using System.Collections.ObjectModel;
using Common.Logging;
using Data.Repositories.Repairs;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid.AddRepair;

namespace Core.Services.RepairsDataGrid;

public class AddRepairService : IAddRepairService
{
    private readonly IAddRepairRepository _repository;
    private readonly IAppLogger<AddRepairService> _logger;

    public AddRepairService(IAddRepairRepository repository, IAppLogger<AddRepairService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<ObservableCollection<EquipmentItem>> GetEquipmentItemsAsync(string equipmentTableName)
    {
        try
        {
            var equipmentsFromDb = await _repository.GetDataAsync(equipmentTableName);
            var equipmentItems = equipmentsFromDb.Select(e => new EquipmentItem
            {
                EquipmentId = e.EquipmentId,
                EquipmentInventoryNumber = e.EquipmentInventoryNumber,
                EquipmentBrand = e.EquipmentBrand,
                EquipmentModel = e.EquipmentModel
            });
            return new ObservableCollection<EquipmentItem>(equipmentItems);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error adding equipment items");
            throw;
        }
    }

    public Task SaveRepairAsync(EquipmentItem equipmentItem, ConsumableItem consumableItem, string equipmentTableName)
    {
        try
        {
            var equipmentDto = new EquipmentDto
            {
                EquipmentId = equipmentItem.EquipmentId,
            };
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error saving repair");
            throw;
        }
    }
}