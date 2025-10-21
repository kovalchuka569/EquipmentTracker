using System;
using System.Diagnostics;
using System.IO;
using Common.Enums;
using Microsoft.Win32;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;
using Presentation.Interfaces;
using Presentation.Services.Interfaces;
using Resources.Localization;

namespace Presentation.Services;

public class ExcelExportManager : IExcelExportManager
{
    private static ISnackbarService _snackbarService;
    public void ExportToExcel(SfDataGrid dataGrid, string fileName, ISnackbarService snackbarService)
    {
        _snackbarService = snackbarService;
        
        if(string.IsNullOrEmpty(fileName)) 
            fileName = "Експорт від " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

        try
        {
            var filePath = ShowSaveFileDialog(fileName);
            if (string.IsNullOrEmpty(filePath))
                return;
            
            var options = CreateDefaultExportOptions();
            ExportDataToExcel(dataGrid, filePath, options);
            ShowSuccessMessage(filePath);
            
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            ShowErrorMessage(e);
            throw;
        }
    }
    
    private static ExcelExportingOptions CreateDefaultExportOptions()
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
    
    private static void ExportDataToExcel(SfDataGrid dataGrid, string filePath, ExcelExportingOptions options)
    {
        using var excelEngine = dataGrid.ExportToExcel(dataGrid.View, options);
        var workbook = excelEngine.Excel.Workbooks[0];
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
        workbook.SaveAs(stream);
    }
    
    private static string ShowSaveFileDialog(string defaultFileName)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel Files|*.xlsx",
            DefaultExt = "xlsx",
            FileName = defaultFileName
        };

        return (saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null) ?? string.Empty;
    }
    
    private static void ShowSuccessMessage(string filePath)
    {
        _snackbarService
            .Show()
            .WithMessage(string.Format(SnackbarMessages.ExportedIn_SuccessMessage_Format, filePath))
            .OfType(SnackType.Warning)
            .Now();
    }

    private static void ShowErrorMessage(Exception ex)
    {
        _snackbarService
            .Show()
            .WithMessage(SnackbarMessages.Export_ErrorMessage)
            .OfType(SnackType.Warning)
            .Now();
    }
}