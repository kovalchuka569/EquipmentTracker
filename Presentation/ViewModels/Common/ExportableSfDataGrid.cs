using Presentation.Models;
using Prism.Commands;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Presentation.ViewModels.Common;

/// <summary>
/// Base ViewModel for Syncfusion SfDataGrid with printing and export capabilities.<br/>
/// Provides common commands and parameters for grid operations.<br/> <br/>
/// Implementations in markup:<br/>
/// <see cref="Presentation.Behaviors.SfDataGridPrintBehavior"/> - for printing with custom settings.<br/>
/// - for exporting to pdf with custom settings.<br/>
/// - for exporting to excel with custom setings.<br/>
/// </summary>
/// <typeparam name="T">The type of the derived ViewModel</typeparam>
public abstract class ExportableSfDataGrid : ViewModelBase
{

    private DelegateCommand? _gridPrintCommand;
    private DelegateCommand? _exportToExcelCommand;
    private DelegateCommand? _exportToPdfCommand;

    private GridPrintParameters? _gridPrintParameters;
    private GridExcelExportParameters? _gridExcelExportParameters;

    /// <summary>
    /// Command to trigger printing of the DataGrid.<br/>
    /// Raises CanExecuteChanged to notify PrintBehavior when executed.<br/>
    /// Need to implement <see cref="Presentation.Behaviors.SfDataGridPrintBehavior"/> in the markup.
    /// </summary>
    public virtual DelegateCommand GridPrintCommand =>
        _gridPrintCommand ??= new DelegateCommand(OnGridPrintCommandExecuted, CanPrint);

    /// <summary>
    /// Parameters for configuring DataGrid printing behavior.
    /// Includes settings like page orientation, paper size, and content selection.
    /// </summary>
    public virtual GridPrintParameters GridPrintParameters
    {
        get => _gridPrintParameters ??= GetGridPrintParameters();
        set => SetProperty(ref _gridPrintParameters, value);
    }

    public virtual GridExcelExportParameters GridExcelExportParameters
    {
        get => _gridExcelExportParameters ??= GetGridExcelExportParameters();
        set => SetProperty(ref _gridExcelExportParameters, value);
    }

    /// <summary>
    /// Command to export DataGrid data to Excel format.
    /// </summary>
    public virtual DelegateCommand ExportToExcelCommand =>
        _exportToExcelCommand ??= new DelegateCommand(OnExportToExcelCommandExecuted, CanExportToExcel);
    
    /// <summary>
    /// Command to export DataGrid data to PDF format.
    /// </summary>
    public virtual DelegateCommand ExportToPdfCommand =>
        _exportToPdfCommand ??= new DelegateCommand(OnExportToPdfCommandExecuted, CanExportToPdf);

    /// <summary>
    /// Executes when the print command is invoked.
    /// Default implementation raises CanExecuteChanged to trigger PrintBehavior.
    /// Override to add custom print logic before triggering the behavior.
    /// </summary>
    protected virtual void OnGridPrintCommandExecuted()
    {
        GridPrintCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Determines whether the print command can execute.
    /// Override to add custom validation logic.
    /// </summary>
    /// <returns>True if printing is allowed; otherwise, false</returns>
    protected virtual bool CanPrint()
    {
        return true;
    }

    /// <summary>
    /// Provides default print parameters for the DataGrid.
    /// Override to customize print settings for specific ViewModels.
    /// </summary>
    /// <returns>Configured GridPrintParameters instance</returns>
    protected virtual GridPrintParameters GetGridPrintParameters()
    {
        return new GridPrintParameters();
    }

    protected virtual GridExcelExportParameters GetGridExcelExportParameters()
    {
        return new GridExcelExportParameters
        {
            ExcelExportingOptions = GetExcelExportingOptions(),
            Path = GetExcelExportPath(),
            Progress = null
        };
    }

    protected virtual ExcelExportingOptions GetExcelExportingOptions()
    {
        return new ExcelExportingOptions
        {
            ExcelVersion = ExcelVersion.Excel2016,
            ExportAllPages = true,
            ExportMode = ExportMode.Text,
            ExportStackedHeaders = true,
            ExportMergedCells = true,
            ExportPageOptions = ExportPageOptions.ExportToDifferentSheets,
        };
    }

    protected virtual string GetExcelExportPath()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = @"Excel Files|*.xlsx",
            DefaultExt = "xlsx",
            FileName = GetExcelExportFileName(),
        };

        return (saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null) ?? string.Empty;
    }

    protected virtual string GetExcelExportFileName()
    {
        return "ExportToExcel";
    }

    /// <summary>
    /// Executes when the Excel export command is invoked.
    /// Default implementation raises CanExecuteChanged.
    /// Override to add custom export logic.
    /// </summary>
    protected virtual void OnExportToExcelCommandExecuted()
    {
        ExportToExcelCommand.RaiseCanExecuteChanged();
    }
    
    /// <summary>
    /// Determines whether the Excel export command can execute.
    /// Override to add custom validation logic.
    /// </summary>
    /// <returns>True if Excel export is allowed; otherwise, false</returns>
    protected virtual bool CanExportToExcel()
    {
        return true;
    }
    
    /// <summary>
    /// Executes when the PDF export command is invoked.
    /// Default implementation raises CanExecuteChanged.
    /// Override to add custom export logic.
    /// </summary>
    protected virtual void OnExportToPdfCommandExecuted()
    {
        ExportToPdfCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Determines whether the PDF export command can execute.
    /// Override to add custom validation logic.
    /// </summary>
    /// <returns>True if PDF export is allowed; otherwise, false</returns>
    protected virtual bool CanExportToPdf()
    {
        return true;
    }
}