using System;
using Presentation.Models;
using Prism.Commands;
using Syncfusion.UI.Xaml.Grid.Converter;

namespace Presentation.Interfaces;

public interface ISfDataGridExportManager
{
    /// <summary>
    /// Command to trigger printing of the DataGrid.
    /// </summary>
    DelegateCommand GridPrintCommand { get; }
    
    /// <summary>
    /// Command to export DataGrid data to Excel format.
    /// </summary>
    DelegateCommand ExportToExcelCommand { get; }
    
    /// <summary>
    /// Command to export DataGrid data to PDF format.
    /// </summary>
    DelegateCommand ExportToPdfCommand { get; }
    
    /// <summary>
    /// Parameters for configuring DataGrid printing behavior.
    /// </summary>
    GridPrintParameters GridPrintParameters { get; set; }
    
    /// <summary>
    /// Parameters for configuring DataGrid Excel export.
    /// </summary>
    GridExcelExportParameters GridExcelExportParameters { get; set; }
    
    /// <summary>
    /// Function for obtaining the file name when exporting.
    /// </summary>
    Func<string> GetExcelFileName { get; set; }
    
    /// <summary>
    /// Function for validating printability.
    /// </summary>
    Func<bool> CanPrint { get; set; }
    
    /// <summary>
    /// Function for validating the ability to export to Excel.
    /// </summary>
    Func<bool> CanExportToExcel { get; set; }
    
    /// <summary>
    /// Function for validating the ability to export to PDF.
    /// </summary>
    Func<bool> CanExportToPdf { get; set; }
    
    /// <summary>
    /// Callback called after successful export to Excel.
    /// </summary>
    Action? OnExcelExported { get; set; }
    
    /// <summary>
    /// Callback called after successful printing.
    /// </summary>
    Action? OnPrinted { get; set; }
    
    /// <summary>
    /// Callback called after successful export to PDF.
    /// </summary>
    Action? OnPdfExported { get; set; }
    
    /// <summary>
    /// Configures custom print settings.
    /// </summary>
    ISfDataGridExportManager ConfigurePrintParameters(Action<GridPrintParameters> configure);
    
    /// <summary>
    /// Configures custom export settings in Excel.
    /// </summary>
    ISfDataGridExportManager ConfigureExcelExportOptions(Action<ExcelExportingOptions> configure);
}