using System;
using System.Diagnostics;
using System.IO;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;

using Notification.Wpf;

using Presentation.Interfaces;


namespace Presentation.UIManagers;

public class PdfExportManager : IPdfExportManager
{

    private static NotificationManager _notificationManager;
    
    public void ExportToPdf(SfDataGrid dataGrid, string fileName, NotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
        
        if(string.IsNullOrEmpty(fileName)) 
            fileName = "Експорт від " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

        try
        {
            var filePath = ShowSaveFileDialog(fileName);
            if (string.IsNullOrEmpty(filePath))
                return;
            
            var options = CreateDefaultExportOptions();
            
            ExportDataToPdf(dataGrid, filePath, options);
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
    
    private static PdfExportingOptions CreateDefaultExportOptions()
    {
        return new PdfExportingOptions
        {
            RepeatHeaders = false,
            FitAllColumnsInOnePage = true,
            ExportAllPages = true,
            ExportStackedHeaders = true,
            ExportGroupSummary = true,
            ExportMergedCells = true,
            ExportGroups = true,
        };
    }
    
    private static void ExportDataToPdf(SfDataGrid dataGrid, string filePath, PdfExportingOptions options)
    {
        using (var pdfDocument = dataGrid.ExportToPdf(dataGrid.View, options))
        {
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                pdfDocument.Save(stream);
            }
        }
    }
    
    private static string ShowSaveFileDialog(string defaultFileName)
    {
        var saveFileDialog = new SaveFileDialog()
        {
            Filter = "PDF Files|*.pdf",
            DefaultExt = "pdf",
            FileName = defaultFileName
        };

        return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
    }
    
    private static void ShowSuccessMessage(string filePath)
    {
        _notificationManager.Show(
            $"Експорт завершено",
            $"Дані успішно збережно в \n{filePath}", NotificationType.Success);
    }
    
    private static void ShowErrorMessage(Exception ex)
    {
        _notificationManager.Show(
            $"Помилка",
            $"Помилка при експорту в \n{ex.Message}", NotificationType.Error);
    }
}