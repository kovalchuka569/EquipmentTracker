using System;
using Microsoft.Win32;
using Presentation.Interfaces;
using Presentation.Models;
using Prism.Commands;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;

namespace Presentation.Services;

public class SfDataGridExportManager : ISfDataGridExportManager
{
    private DelegateCommand? _gridPrintCommand;
    private DelegateCommand? _gridExportToExcelCommand;
    private DelegateCommand? _gridExportToPdfCommand;
    
    private GridPrintParameters? _gridPrintParameters;
    private GridExcelExportParameters? _gridExcelExportParameters;
    
    public Func<string> GetExcelFileName { get; set; } = () => "ExportToExcel";
    
    public Func<bool> CanPrint { get; set; } = () => true;

    public Func<bool> CanExportToExcel { get; set; } = () => true;
    
    public Func<bool> CanExportToPdf { get; set; } = () => true;

    public Action? OnExcelExported { get; set; }
    
    public Action? OnPrinted { get; set; }
    
    public Action? OnPdfExported { get; set; }
    
    #region Commands
    
    public DelegateCommand GridPrintCommand =>
        _gridPrintCommand ??= new DelegateCommand(OnGridPrintCommandExecuted, CanPrint);
    
    public DelegateCommand ExportToExcelCommand =>
        _gridExportToExcelCommand ??= new DelegateCommand(OnExportToExcelCommandExecuted, CanExportToExcel);
    
    public DelegateCommand ExportToPdfCommand =>
        _gridExportToPdfCommand ??= new DelegateCommand(OnExportToPdfCommandExecuted, CanExportToPdf);

    #endregion
    
    #region Parameters
    
    public GridPrintParameters GridPrintParameters
    {
        get => _gridPrintParameters ??= CreateDefaultPrintParameters();
        set => _gridPrintParameters = value;
    }
    
    public GridExcelExportParameters GridExcelExportParameters
    {
        get => _gridExcelExportParameters ??= CreateDefaultExcelExportParameters();
        set => _gridExcelExportParameters = value;
    }

    #endregion
    
    private void OnGridPrintCommandExecuted()
    {
        Console.WriteLine("Grid Print");
        GridPrintCommand.RaiseCanExecuteChanged();
    }
    
    private void OnExportToExcelCommandExecuted()
    {
        ExportToExcelCommand.RaiseCanExecuteChanged();
    }
    
    private void OnExportToPdfCommandExecuted()
    {
        ExportToPdfCommand.RaiseCanExecuteChanged();
    }
    
    #region Factory Methods
    
    protected virtual GridPrintParameters CreateDefaultPrintParameters()
    {
        return new GridPrintParameters();
    }

    protected virtual GridExcelExportParameters CreateDefaultExcelExportParameters()
    {
        return new GridExcelExportParameters
        {
            ExcelExportingOptions = CreateDefaultExcelExportingOptions(),
            Path = GetExcelExportPath(),
            Progress = null
        };
    }

    protected virtual ExcelExportingOptions CreateDefaultExcelExportingOptions()
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
            FileName = GetExcelFileName(),
        };

        return (saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null) ?? string.Empty;
    }

    #endregion
    
    #region Public Configuration Methods
    
    public ISfDataGridExportManager ConfigurePrintParameters(Action<GridPrintParameters> configure)
    {
        var parameters = GridPrintParameters;
        configure(parameters);
        GridPrintParameters = parameters;
        return this;
    }
    
    public ISfDataGridExportManager ConfigureExcelExportOptions(Action<ExcelExportingOptions> configure)
    {
        var parameters = GridExcelExportParameters;
        configure(parameters.ExcelExportingOptions);
        GridExcelExportParameters = parameters;
        return this;
    }
    
    #endregion
}