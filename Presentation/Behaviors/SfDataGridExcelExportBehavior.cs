using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Presentation.Models;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;

namespace Presentation.Behaviors;

public class SfDataGridExcelExportBehavior : Behavior<SfDataGrid>
{
    public static readonly DependencyProperty GridExcelExportCommandProperty = DependencyProperty.Register(
        nameof(GridExcelExportCommand),
        typeof(ICommand), 
        typeof(SfDataGridExcelExportBehavior), 
        new PropertyMetadata(null, OnGridExcelExportCommandChanged));

    public static readonly DependencyProperty GridExcelExportParametersProperty = DependencyProperty.Register(
        nameof(GridExcelExportParameters),
        typeof(GridExcelExportParameters), typeof(SfDataGridExcelExportBehavior));
    
    public ICommand? GridExcelExportCommand
    {
        get => (ICommand)GetValue(GridExcelExportCommandProperty);
        set => SetValue(GridExcelExportCommandProperty, value);
    }

    public GridExcelExportParameters? GridExcelExportParameters
    {
        get => (GridExcelExportParameters)GetValue(GridExcelExportParametersProperty);
        set => SetValue(GridExcelExportParametersProperty, value);
    }

    private static void OnGridExcelExportCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (SfDataGridExcelExportBehavior)d;
        
        if (e.OldValue is ICommand oldCommand) oldCommand.CanExecuteChanged -= behavior.OnCanExecuteChanged;

        if (e.NewValue is ICommand newCommand) newCommand.CanExecuteChanged += behavior.OnCanExecuteChanged;
    }
    
    private async void OnCanExecuteChanged(object? sender, System.EventArgs e)
    {
        if (GridExcelExportCommand?.CanExecute(null) is not true)
            return;
        
        if (GridExcelExportParameters is null)
            return;
        
        await ExportToExcel(GridExcelExportParameters);
    }

    private async Task ExportToExcel(GridExcelExportParameters exportParameters)
    {
        try
        {
            using var excelEngine = AssociatedObject.ExportToExcel(AssociatedObject.View, exportParameters.ExcelExportingOptions);
            var workbook = excelEngine.Excel.Workbooks[0];
            await using var stream = new FileStream(exportParameters.Path, FileMode.Create, FileAccess.ReadWrite);
            workbook.SaveAs(stream);
            exportParameters.OnSuccess.Invoke();
        }
        catch (Exception e)
        {
            exportParameters.OnError.Invoke(e);
        }
        finally
        {
            exportParameters.OnFinally.Invoke();
        }
    }
}