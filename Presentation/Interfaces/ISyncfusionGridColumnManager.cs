using System.Windows;

using Syncfusion.UI.Xaml.Grid;

using Models.Common.Table;

namespace Presentation.Interfaces;

public interface ISyncfusionGridColumnManager
{
    GridColumn CreateColumn(ColumnModel columnModel, Style basedGridHeaderStyle);
}