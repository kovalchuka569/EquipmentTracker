using System.Collections.ObjectModel;
using Common.Logging;
using Data.Repositories.Repairs;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid;
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

    public async Task<int> SaveRepairAsync(RepairData repairData, string equipmentTableName)
    {
        try
        {
            return await _repository.SaveRepairAsync(repairData, equipmentTableName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error saving repair");
            throw;
        }
    }

    public async Task InsertUsedMaterialsAsync(ObservableCollection<RepairConsumableItem> repairConsumableItems, int repairId, string consumablesTableName)
    {
        try
        {
            _logger.LogInformation("Inserting dto used materials");
            var repairConsumableDto = new ObservableCollection<RepairConsumableDto>(repairConsumableItems.Select(item =>
                new RepairConsumableDto
                {
                    ConsumableTableName = item.ConsumableTableName,
                    Name = item.Name,
                    Unit = item.Unit,
                    Category = item.Category,
                    SpentMaterial = item.SpentMaterial
                }));
            _logger.LogInformation("Successfully inserting dto used materials");
            _logger.LogInformation("Sending used materials to repository");
            await _repository.InsertUsedMaterialsAsync(repairConsumableDto, repairId, consumablesTableName);
            _logger.LogInformation("Successfully sending used materials to repository");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error inserting dto or sending to repository used materials");
            throw;
        }
    }
}