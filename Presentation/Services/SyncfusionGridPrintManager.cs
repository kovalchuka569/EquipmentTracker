using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Syncfusion.UI.Xaml.Grid;
using Presentation.Interfaces;

namespace Presentation.Services;

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
            FontSize = 10,
            BorderBrush = Brushes.LightGray,
        };
    }

    protected override TextWrapping GetColumnTextWrapping(string mappingName)
    {
        return TextWrapping.Wrap;
    }

    protected override double GetRowHeight(object? record, int rowIndex, RowType rowType)
    {
        if (record == null) 
            return base.GetRowHeight(record, rowIndex, rowType);
        
        var actualRowIndex = dataGrid.ResolveToRowIndex(rowIndex);
        
        return dataGrid.GridColumnSizer.GetAutoRowHeight(actualRowIndex, _gridRowSizingOptions, out _height) 
            ? _height 
            : base.GetRowHeight(record, rowIndex, rowType);
    }
}