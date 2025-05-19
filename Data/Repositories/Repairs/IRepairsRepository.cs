using System.Collections.ObjectModel;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;

namespace Data.Repositories.Repairs
{
    public interface IRepairsRepository
    {
        Task<List<RepairDto>> GetDataAsync(string tableName, string equipmentTable);
    }
}

