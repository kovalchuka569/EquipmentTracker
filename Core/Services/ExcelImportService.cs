using System.Globalization;
using System.IO;
using Syncfusion.XlsIO;

using Core.Interfaces;

using Models.Common.Table;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
using Models.Services;

namespace Core.Services;

public class ExcelImportService : IExcelImportService
{
    public async Task<List<RowModel>> ImportRowsAsync(ExcelImportConfig importConfig)
    {
        if (!File.Exists(importConfig.FilePath)) throw new FileNotFoundException("File not found");

        var result = new List<RowModel>();

        try
        { 
            using var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            
            using var fileStream = new FileStream(importConfig.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var memoryStream = new MemoryStream();
            
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            var workbook = application.Workbooks.Open(memoryStream);
            var worksheet = workbook.Worksheets[importConfig.SheetName];
            
            if (worksheet is null) throw new InvalidOperationException("Sheet not found");
            
            var excelHeaders = GetExcelHeaders(worksheet, importConfig.HeadersRange);
            
            var headerMapping = CreateHeaderMappings(
                excelHeaders, 
                importConfig.AvailableColumns
                    .Select(c => (c.HeaderText, c.MappingName))
                    .ToList()
            );

            var startRow = Math.Max(importConfig.RowRangeStart, 1);
            var endRow = Math.Min(importConfig.RowRangeEnd, worksheet.UsedRange.LastRow);
            
            for (var rowIndex = startRow; rowIndex <= endRow; rowIndex++)
            {
                var cells = new List<CellModel>();
                var hasData = false;

                foreach (var availableColumn in importConfig.AvailableColumns)
                {
                    var cellModel = new CellModel
                    {
                        ColumnMappingName = availableColumn.MappingName
                    };

                    if (headerMapping.TryGetValue(availableColumn.HeaderText, out var excelColumnIndex))
                    {
                        var excelCell = worksheet[rowIndex, excelColumnIndex];
                        cellModel.Value = GetCellValueByType(excelCell, availableColumn.ColumnType);

                        if (cellModel.Value != null && !string.IsNullOrWhiteSpace(cellModel.Value.ToString()))
                        {
                            hasData = true;
                        }
                    }
                    else
                    {
                        cellModel.Value = null;
                    }
                    
                    cells.Add(cellModel);
                }

                if (!hasData) continue;
                
                var rowModel = new RowModel
                {
                    Cells = cells
                };

                foreach (var cell in rowModel.Cells)
                {
                    cell.RowId = rowModel.Id;
                }
                    
                result.Add(rowModel);
            }
            workbook.Close();
            
            return result;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to import Excel data: {e.Message}", e);
        }
    }

    private static Dictionary<string, int> GetExcelHeaders(IWorksheet worksheet, string headersRange)
    {
        var headers = new Dictionary<string, int>();

        var range = worksheet.Range[headersRange];
        var startCol = range.Column;
        var endCol = range.LastColumn;
        var row = range.Row;

        for (var col = startCol; col <= endCol; col++)
        {
            var headerValue = worksheet[row, col].DisplayText?.Trim();
            if (!string.IsNullOrEmpty(headerValue))
            {
                headers[headerValue] = col;
            }
        }
        
        return headers;
    }

    private static Dictionary<string, int> CreateHeaderMappings(Dictionary<string, int> excelHeaders, List<(string, string)> availableHeaders)
    {
        var mapping = new Dictionary<string, int>();

        foreach (var header in availableHeaders)
        {
            var matchingExcelHeader = excelHeaders.Keys.FirstOrDefault(eh =>
                string.Equals(eh, header.Item1, StringComparison.OrdinalIgnoreCase));

            if (matchingExcelHeader != null)
            {
                mapping[header.Item1] = excelHeaders[matchingExcelHeader];
            }
        }
        
        return mapping;
    }

    private static object? GetCellValueByType(IRange? cell, ColumnDataType expectedType)
    {
        if (cell is null || string.IsNullOrEmpty(cell.DisplayText))
            return null;
        
        var textValue = cell.DisplayText.Trim();

        try
        {
            switch (expectedType)
            {
                case ColumnDataType.Number:
                    return ParseNumber(textValue);
                    
                case ColumnDataType.Currency:
                    return ParseCurrency(textValue);
                
                case ColumnDataType.Date:
                    return ParseDate(textValue);
                
                case ColumnDataType.Boolean:
                    return ParseBool(textValue);
                
                case ColumnDataType.List:
                case ColumnDataType.Text:
                case ColumnDataType.Hyperlink:
                default:
                    return textValue;
            }
        }
        catch
        {
            return null;
        }
    }
    
    private static double? ParseNumber(string input)
    {
        var cleaned = CleanNumberString(input);
        if (string.IsNullOrEmpty(cleaned)) return null;

        if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.CurrentCulture, out var val))
            return val;

        if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
            return val;

        return null;
    }
    
    private static decimal? ParseCurrency(string input)
    {
        var cleaned = CleanNumberString(input);
        if (string.IsNullOrEmpty(cleaned)) return null;

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.CurrentCulture, out var val))
            return val;

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
            return val;

        return null;
    }
    
    private static DateTime? ParseDate(string input)
    {
        if (DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.None, out var date))
            return date;
        
        if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return date;

        var formatsArray = ComboBoxDateFormat
            .GetComboBoxDateFormats()
            .ToList()
            .Select(format => format.Format)
            .ToArray();

        if (DateTime.TryParseExact(input, formatsArray, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return date;

        return null;
    }
    
    private static bool? ParseBool(string input)
    {
        input = input.Trim().ToLowerInvariant();

        if (bool.TryParse(input, out var boolVal))
            return boolVal;

        return input switch
        {
            "yes" or "y" or "да" or "true" or "1" or "так" or "вкл" => true,
            "no" or "n" or "нет" or "false" or "0" or "ні" or "выкл" or "викл" => false,
            _ => null
        };
    }
    
    private static string CleanNumberString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var allowedChars = "0123456789-.,";
        var result = new System.Text.StringBuilder();

        foreach (var c in input.Where(c => allowedChars.Contains(c)))
        {
            result.Append(c);
        }

        return result.ToString().Replace(',', '.');
    }
}