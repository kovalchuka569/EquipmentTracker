using System.Collections.ObjectModel;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;

namespace Core.Services.RepairsDataGrid;

public interface IRepairsDataGridService
{ 
    Task<ObservableCollection<RepairItem>> GetRepairItems(string tableName, string equipmentTableName);
}