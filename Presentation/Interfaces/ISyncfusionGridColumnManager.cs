using System.Windows;
using Models.Common.Table.ColumnProperties;
using Syncfusion.UI.Xaml.Grid;

using Presentation.ViewModels.Common.ColumnDesigner;

namespace Presentation.Interfaces;

public interface ISyncfusionGridColumnManager
{
    GridColumn CreateColumn(BaseColumnProperties columnProperties, Style basedGridHeaderStyle);
}