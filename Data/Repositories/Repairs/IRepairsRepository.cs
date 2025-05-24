using System.Collections.ObjectModel;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Models.RepairsDataGrid.AddService;

namespace Data.Repositories.Repairs
{
    public interface IRepairsRepository
    {
        Task<List<RepairDto>> GetDataAsync(string repairsTableName, string equipmentsTableName);
    }
}

