using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Syncfusion.UI.Xaml.Grid;

using Presentation.Interfaces;

namespace Presentation.UIManagers;

public class SyncfusionGridPrintManager : GridPrintManager, ISyncfusionGridPrintManager
{
    private readonly GridRowSizingOptions _gridRowSizingOptions = new();
    private double _height = double.NaN;
    
    public SyncfusionGridPrintManager(SfDataGrid grid)
        : base(grid)
    {
        grid.PrintSettings.AllowColumnWidthFitToPrintPage = false;
        grid.PrintSettings.AllowPrintByDrawing = false;
        grid.PrintSettings.AllowRepeatHeaders = false;
        grid.PrintSettings.CanPrintStackedHeaders = true;
        grid.AllowGrouping = true;
        grid.PrintSettings.PrintPageOrientation = PrintOrientation.Landscape;
    }

    public override ContentControl GetPrintHeaderCell(string mappingName)
    {
        return new PrintHeaderCell
        {
            FontFamily = new FontFamily("Microsoft Sans Serif"),
            FontSize = 10,
            FontWeight = FontWeights.DemiBold,
            BorderBrush = Brushes.LightGray,
            Background = Brushes.White
        };
    }

    public override ContentControl GetPrintGridCell(object record, string mappingName)
    {
        return new PrintGridCell
        {
            FontFamily = new FontFamily("Microsoft Sans Serif"),
            FontSize = 10,
            BorderBrush = Brushes.LightGray,
        };
    }

    protected override TextWrapping GetColumnTextWrapping(string mappingName)
    {
        return TextWrapping.Wrap;
    }

    protected override double GetRowHeight(object record, int rowIndex, RowType rowType)
    {
        if(record != null)

        {
            var actualrowindex = GridIndexResolver.ResolveToRowIndex(dataGrid, rowIndex);
            if (dataGrid.GridColumnSizer.GetAutoRowHeight(actualrowindex, _gridRowSizingOptions, out _height))
            {
                return _height;
            }
        }
        return base.GetRowHeight(record, rowIndex, rowType);
    }
}