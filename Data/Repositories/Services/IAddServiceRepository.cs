using Models.RepairsDataGrid.AddRepair;
using Models.RepairsDataGrid.AddService;
using Models.RepairsDataGrid.ServicesDataGrid;

namespace Data.Repositories.Services;

public interface IAddServiceRepository
{
    Task<List<EquipmentDto>> GetDataAsync (string equipmentTableName);
    Task<int> SaveServiceAsync (ServiceData serviceData, string servicesTableName);
    Task UpdateServiceAsync (ServiceData serviceData, string servicesTableName, int serviceId);
}