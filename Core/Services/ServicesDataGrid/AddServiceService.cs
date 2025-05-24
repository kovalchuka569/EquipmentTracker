using System.Collections.ObjectModel;
using Common.Logging;
using Data.Repositories.Repairs;
using Data.Repositories.Services;
using Models.RepairsDataGrid.AddRepair;
using Models.RepairsDataGrid.AddService;

namespace Core.Services.ServicesDataGrid;

public class AddServiceService : IAddServiceService
{
    private readonly IAppLogger<AddServiceService> _logger;
    private readonly IAddServiceRepository _repository;
    public AddServiceService(IAppLogger<AddServiceService> logger, IAddServiceRepository repository)
    {
        _logger = logger;
        _repository = repository;
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
    
    public async Task<int> SaveRepairAsync(ServiceData serviceData, string equipmentTableName)
    {
        try
        {
            return await _repository.SaveServiceAsync(serviceData, equipmentTableName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error saving service");
            throw;
        }
    }

    public async Task UpdateServiceAsync(ServiceData serviceData, string servicesTableName, int serviceId)
    {
        try
        {
            _repository.UpdateServiceAsync(serviceData, servicesTableName, serviceId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error updating service");
            throw;
        }
    }
}