using Models.Equipment.ColumnSettings;
using Syncfusion.UI.Xaml.Grid;

namespace EquipmentTracker.Factories.Interfaces;

public interface IGridColumnFactory
{
    GridColumn CreateColumn(ColumnSettingsDisplayModel settings);
}