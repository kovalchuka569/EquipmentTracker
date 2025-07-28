using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Syncfusion.UI.Xaml.Grid;

namespace EquipmentTracker.Common.DataGridExport;

public class PrintManager : GridPrintManager
{
    
    GridRowSizingOptions gridRowSizingOptions = new();
    double Height = double.NaN;
    SfDataGrid dataGrid;
    
    public PrintManager(SfDataGrid grid)
        : base(grid)
    {
        dataGrid = grid;
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
            if (dataGrid.GridColumnSizer.GetAutoRowHeight(actualrowindex, gridRowSizingOptions, out Height))
            {
                return Height;
            }
        }
        return base.GetRowHeight(record, rowIndex, rowType);
    }
}