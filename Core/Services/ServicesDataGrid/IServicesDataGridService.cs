using System.Collections.ObjectModel;
using Models.RepairsDataGrid.ServicesDataGrid;

namespace Core.Services.ServicesDataGrid;

public interface IServicesDataGridService
{
    Task<ObservableCollection<ServiceItem>> GetServiceItems(string servicesTableName, string equipmentsTableName);
}