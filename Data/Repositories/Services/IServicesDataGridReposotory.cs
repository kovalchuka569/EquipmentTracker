using Models.RepairsDataGrid.ServicesDataGrid;

namespace Data.Repositories.Services;

public interface IServicesDataGridReposotory
{
    Task<List<ServiceDto>> GetDataAsync(string servicesTablename, string equipmentsTable);
}