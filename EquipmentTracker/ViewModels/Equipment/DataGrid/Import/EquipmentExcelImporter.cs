using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Services.EquipmentDataGrid;
using Models.Equipment;
using Models.Equipment.ColumnSettings;
using Models.Equipment.ColumnSpecificSettings;
using Syncfusion.XlsIO;

namespace EquipmentTracker.ViewModels.Equipment.DataGrid.Import;

/// <summary>
/// Handles importing data from an Excel file and inserting it into the equipment data-grid.
/// </summary>
public sealed class EquipmentExcelImporter
{
    public record ImportResult(int Imported, int Skipped, List<Dictionary<string, object?>> ImportedData);

    private readonly IEquipmentSheetService _service;
    private readonly Dictionary<string, ColumnItem> _columnsByHeader;
    private readonly Guid _tableId;

    public EquipmentExcelImporter(IEquipmentSheetService service, IEnumerable<ColumnItem> columns, Guid tableId)
    {
        _service = service;
        _tableId = tableId;
        _columnsByHeader = columns.ToDictionary(c => c.Settings.HeaderText, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Imports the provided Excel file.
    /// </summary>
    public async Task<ImportResult> ImportAsync(string filePath, string? sheetName = null, 
        int headerRow = 1, int headerCol = 1)
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        
        try
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var workbook = application.Workbooks.Open(stream);
            
            var sheet = sheetName == null 
                ? workbook.Worksheets[0] 
                : workbook.Worksheets.FirstOrDefault(ws => string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                  ?? throw new FileNotFoundException($"Лист '{sheetName}' не знайдено");
            
            var columnMap = CreateColumnMapping(sheet, headerRow, headerCol);
            
            if (columnMap.Count == 0)
            {
                workbook.Close();
                return new ImportResult(0, 0, new());
            }
            
            var result = await ProcessDataRowsAsync(sheet, headerRow, columnMap);
            
            workbook.Close();
            return result;
        }
        catch (IOException ex) when (IsFileLockedException(ex))
        {
            throw new IOException("Файл зайнятий іншим процесом. Будь ласка, закрийте його перед імпортом.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Помилка імпорту: {ex.Message}", ex);
        }
    }

    private Dictionary<int, ColumnItem> CreateColumnMapping(IWorksheet sheet, int headerRow, int headerCol)
    {
        var map = new Dictionary<int, ColumnItem>();
        var lastCol = sheet.UsedRange.LastColumn;
        
        for (int c = headerCol; c <= lastCol; c++)
        {
            var header = sheet.Range[headerRow, c].Value?.ToString()?.Trim();
            
            if (!string.IsNullOrEmpty(header) && _columnsByHeader.TryGetValue(header, out var column))
            {
                map[c] = column;
            }
        }
        
        return map;
    }
    
    private Task<ImportResult> ProcessDataRowsAsync(IWorksheet sheet, int headerRow, Dictionary<int, ColumnItem> columnMap)
    {
        var importedData = new List<Dictionary<string, object?>>();
        int imported = 0, skipped = 0;
        int lastRow = sheet.UsedRange.LastRow;
        int consecutiveEmptyRows = 0;
        const int maxEmptyRows = 20;

        for (int r = headerRow + 1; r <= lastRow; r++)
        {
            var rowData = ProcessRow(sheet, r, columnMap);
            
            if (rowData.Count == 0)
            {
                consecutiveEmptyRows++;
                if (consecutiveEmptyRows >= maxEmptyRows) 
                    break;
                
                skipped++;
                continue;
            }

            consecutiveEmptyRows = 0;
            
            try
            {
                importedData.Add(rowData);
                imported++;
            }
            catch (Exception ex)
            {
                skipped++;
            }
        }

        return Task.FromResult(new ImportResult(imported, skipped, importedData));
    }
    
    private Dictionary<string, object?> ProcessRow(IWorksheet sheet, int rowNum, Dictionary<int, ColumnItem> columnMap)
    {
        var rowData = new Dictionary<string, object?>();
        

        foreach (var (colIndex, columnItem) in columnMap)
        {
            var cell = sheet.Range[rowNum, colIndex];
            object? rawValue = GetCellValue(cell);
            

            if (rawValue != null)
            {
                var convertedValue = ConvertCellValue(rawValue, columnItem.Settings);
                if (convertedValue != null)
                {
                    rowData[columnItem.Settings.MappingName] = convertedValue;
                }
            }
        }

        return rowData;
    }

    private static object? GetCellValue(IRange cell)
    {
        if (cell.HasFormula)
        {
            if (cell.HasFormulaNumberValue) return cell.FormulaNumberValue;
            if (cell.HasFormulaDateTime) return cell.FormulaDateTime;
            if (cell.HasFormulaStringValue) return cell.FormulaStringValue;
        }
        return cell.Value;
    }

    private static object? ConvertCellValue(object rawValue, ColumnSettingsDisplayModel settings)
    {
        if (rawValue == null) return null;

        try
        {
            string stringValue = rawValue.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(stringValue)) return null;

            return settings.DataType switch
            {
                ColumnDataType.Number => ConvertToNumber(stringValue, settings.SpecificSettings as NumberColumnSettings),
                ColumnDataType.Currency => ConvertToCurrency(stringValue, settings.SpecificSettings as CurrencyColumnSettings),
                ColumnDataType.Boolean => ConvertToBoolean(stringValue, settings.SpecificSettings as BooleanColumnSettings),
                ColumnDataType.Date => ConvertToDate(stringValue, settings.SpecificSettings as DateColumnSettings),
                ColumnDataType.List => ConvertToListValue(stringValue, settings.SpecificSettings as ListColumnSettings),
                ColumnDataType.Text => ConvertToText(stringValue, settings.SpecificSettings as TextColumnSettings),
                ColumnDataType.Hyperlink => stringValue, 
                _ => stringValue
            };
        }
        catch
        {
            return rawValue.ToString();
        }
    }

    private static double? ConvertToNumber(string value, NumberColumnSettings? settings)
    {
        string cleanValue = value.Replace(" ", "").Replace(",", ".");
        
        if (double.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            if (settings != null)
            {
                if (settings.MinValue != 0 && result < settings.MinValue)
                    return settings.MinValue;
                if (settings.MaxValue != 0 && result > settings.MaxValue)
                    return settings.MaxValue;
                
                if (settings.CharactersAfterComma >= 0)
                    return Math.Round(result, settings.CharactersAfterComma);
            }
            return result;
        }
        return null;
    }

    private static double? ConvertToCurrency(string value, CurrencyColumnSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        
        string cleanValue = value.Replace(" ", "").Replace(",", ".");
        
        if (settings?.CurrencySymbol != null)
        {
            cleanValue = cleanValue.Replace(settings.CurrencySymbol, "");
        }
        
        if (double.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }
        return null;
    }

    private static bool? ConvertToBoolean(string value, BooleanColumnSettings? settings)
    {
        var lowerValue = value.ToLowerInvariant();
        return lowerValue switch
        {
            "1" or "true" or "yes" or "y" or "да" or "так" or "истина" => true,
            "0" or "false" or "no" or "n" or "нет" or "ні" or "ложь" => false,
            _ => settings?.DefaultValue
        };
    }

    private static DateTime? ConvertToDate(string value, DateColumnSettings? settings)
    {
        // Пробуем различные форматы даты
        var formats = new[]
        {
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy",
            "yyyy/MM/dd"
        };
        
        if (settings?.DateFormat != null)
        {
            if (DateTime.TryParseExact(value, settings.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime exactDate))
                return exactDate;
        }

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return date;
        }
        
        if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime generalDate))
            return generalDate;

        return null;
    }

    private static string? ConvertToListValue(string value, ListColumnSettings? settings)
    {
        if (settings?.ListValues == null) 
            return value;
        
        if (settings.ListValues.Contains(value))
            return value;
        
        var matchedValue = settings.ListValues.FirstOrDefault(v => 
            string.Equals(v, value, StringComparison.OrdinalIgnoreCase));
        
        if (matchedValue != null)
            return matchedValue;
        
        return settings.DefaultValue ?? value;
    }

    private static string? ConvertToText(string value, TextColumnSettings? settings)
    {
        if (settings == null) return value;
        
        if (settings.MinLength > 0 && value.Length < settings.MinLength)
            return null; 
        
        if (settings.MaxLength > 0 && value.Length > settings.MaxLength)
            return value.Substring(0, (int)settings.MaxLength);

        return value;
    }

    private static string? ConvertToMultilineText(string value, MultilineTextColumnSettings? settings)
    {
        if (settings?.MaxLength > 0 && value.Length > settings.MaxLength)
            return value.Substring(0, (int)settings.MaxLength);

        return value;
    }

    private static bool IsFileLockedException(Exception ex)
    {
        return ex is IOException && (
            ex.Message.Contains("used by another process") ||
            ex.Message.Contains("being used by another process") ||
            ex.HResult == -2147024864 // ERROR_SHARING_VIOLATION
        );
    }
}