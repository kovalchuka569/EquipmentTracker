using System.Windows;
using Models.Common.Table;
using Syncfusion.UI.Xaml.Grid;

namespace EquipmentTracker.Factories.Interfaces;

public interface IGridColumnFactory
{
    GridColumn CreateColumn(ColumnModel columnModel, Style basedGridHeaderStyle);
}