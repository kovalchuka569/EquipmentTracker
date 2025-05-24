using System.Collections.ObjectModel;
using Common.Logging;
using Data.AppDbContext;
using Data.Repositories.Services;
using Models.RepairsDataGrid.ServicesDataGrid;

namespace Core.Services.ServicesDataGrid;

public class ServicesDataGridService : IServicesDataGridService
{
    private readonly IServicesDataGridReposotory _repository;
    private readonly IAppLogger<ServicesDataGridService> _logger;

    public ServicesDataGridService(IServicesDataGridReposotory repository,
        IAppLogger<ServicesDataGridService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<ObservableCollection<ServiceItem>> GetServiceItems(string servicesTableName, string equipmentsTableName)
    {
        try
        {
            var servicesFromDb = await _repository.GetDataAsync(servicesTableName, equipmentsTableName);
            var serviceItems = servicesFromDb.Select(r => new ServiceItem
            {
                Id = r.Id,
                EquipmentId = r.EquipmentId,
                ServiceDescription = r.ServiceDescription,
                EquipmentInventoryNumber = r.EquipmentInventoryNumber,
                EquipmentBrand = r.EquipmentBrand,
                EquipmentModel = r.EquipmentModel,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Duration = r.Duration,
                Worker = r.Worker,
                Status = r.Status,
                Type = r.Type,
            });
            return new ObservableCollection<ServiceItem>(serviceItems);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error inserting repair items");
            throw;
        }
    }
}