using System.Collections.ObjectModel;
using Models.RepairsDataGrid.AddRepair;
using Models.RepairsDataGrid.AddService;

namespace Core.Services.ServicesDataGrid;

public interface IAddServiceService
{
    Task<ObservableCollection<EquipmentItem>> GetEquipmentItemsAsync(string equipmentTableName);
    Task<int> SaveRepairAsync(ServiceData service, string equipmentTableName);
    Task UpdateServiceAsync (ServiceData serviceData, string servicesTableName, int serviceId);
}