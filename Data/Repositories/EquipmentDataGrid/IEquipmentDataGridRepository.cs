using System.Collections.ObjectModel;
using Models.EquipmentDataGrid;

namespace Data.Repositories.EquipmentDataGrid;

public interface IEquipmentDataGridRepository
{
    Task<ObservableCollection<EquipmentDto>> GetDataAsync(string equipmentTableName);
}