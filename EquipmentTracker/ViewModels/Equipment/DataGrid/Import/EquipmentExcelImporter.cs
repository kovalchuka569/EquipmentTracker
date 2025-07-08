using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Services.EquipmentDataGrid;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
using Syncfusion.XlsIO;

namespace UI.ViewModels.Equipment.DataGrid.Import;

/// <summary>
/// Handles importing data from an Excel file and inserting it into the equipment data-grid.
/// </summary>
public sealed class EquipmentExcelImporter
{
    public record ImportResult(int Imported, int Skipped);

    private readonly IEquipmentDataGridService _service;
    private readonly Dictionary<string, ColumnItem> _columnsByHeader;
    private readonly int _tableId;

    public EquipmentExcelImporter(IEquipmentDataGridService service, IEnumerable<ColumnItem> columns, int tableId)
    {
        _service = service;
        _tableId = tableId;
        _columnsByHeader = columns.ToDictionary(c => c.Settings.HeaderText, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Imports the provided Excel file.
    /// </summary>
    public async Task<ImportResult> ImportAsync(string filePath, string? sheetName = null, int headerRow = 1, int headerCol = 1)
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        await using var stream = File.OpenRead(filePath);
        var workbook = application.Workbooks.Open(stream);
        var sheet = sheetName == null ? workbook.Worksheets[0] : workbook.Worksheets.First(ws => string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase));

        // Map Excel column index -> ColumnItem starting from headerCol
        var map = new Dictionary<int, ColumnItem>();
        var lastCol = sheet.UsedRange.LastColumn;
        for (int c = headerCol; c <= lastCol; c++)
        {
            var header = sheet.Range[headerRow, c].Value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(header))
                continue;
            if (_columnsByHeader.TryGetValue(header, out var column))
                map[c] = column;
        }

        int imported = 0, skipped = 0;
        var lastRow = sheet.UsedRange.LastRow;
        int emptyStreak = 0;
        for (int r = headerRow + 1; r <= lastRow; r++)
        {
            var dataDict = new Dictionary<string, object?>();
            bool hasValue = false;
            foreach (var (colIndex, columnItem) in map)
            {
                var raw = sheet.Range[r, colIndex].Value;
                if (raw == null)
                    continue;
                hasValue = true;
                var converted = ConvertCell(raw, columnItem.Settings.DataType);
                dataDict[columnItem.Settings.MappingName] = converted!;
            }
            if (!hasValue)
            {
                emptyStreak++;
                if (emptyStreak >= 50) // assume end of data
                    break;
                skipped++;
                continue;
            }
            emptyStreak = 0;
            var item = new EquipmentItem
            {
                TableId = _tableId,
                RowIndex = r - (headerRow + 1),
                Data = dataDict
            };
            await _service.AddNewRowAsync(item);
            imported++;
        }

        workbook.Close();
        return new ImportResult(imported, skipped);
    }

    private static object? ConvertCell(object raw, ColumnDataType dataType)
    {
        try
        {
            return dataType switch
            {
                ColumnDataType.Number or ColumnDataType.Currency => double.TryParse(raw.ToString(), out var d) ? d : raw,
                ColumnDataType.Boolean => ParseBool(raw),
                ColumnDataType.Date => DateTime.TryParse(raw.ToString(), out var dt) ? dt : raw,
                ColumnDataType.List or ColumnDataType.Text or ColumnDataType.MultilineText or ColumnDataType.Hyperlink => raw.ToString()?.Trim(),
                _ => raw
            };
        }
        catch
        {
            return raw;
        }
    }

    private static bool? ParseBool(object raw)
    {
        var str = raw.ToString()?.Trim().ToLower();
        return str switch
        {
            "1" or "true" or "yes" or "y" or "да" => true,
            "0" or "false" or "no" or "n" or "нет" => false,
            _ => null
        };
    }
}
