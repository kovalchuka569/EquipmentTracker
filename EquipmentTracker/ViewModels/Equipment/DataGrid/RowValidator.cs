using System.Collections.ObjectModel;
using System.Dynamic;
using EquipmentTracker.Common;
using Models.Equipment;
using Models.Equipment.ColumnSpecificSettings;
using Syncfusion.UI.Xaml.Grid;

namespace EquipmentTracker.ViewModels.Equipment.DataGrid;

public class RowValidator
{
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public Dictionary<string, string> ErrorMessages { get; } = new();
    }

    private readonly Dictionary<GridColumnBase, ColumnItem> _columnItemMap;
    private readonly ObservableCollection<ExpandoObject> _rows;

    public RowValidator(Dictionary<GridColumnBase, ColumnItem> columnItemMap, ObservableCollection<ExpandoObject> rows)
    {
        _columnItemMap = columnItemMap;
        _rows = rows;
    }

    public ValidationResult ValidateRow(object rowData)
    {
        var result = new ValidationResult();

        if (rowData is not ExpandoObject equipmentRow)
        {
            return result;
        }
        
        var rowDict = equipmentRow as IDictionary<string, object>;

        foreach (var kvp in _columnItemMap)
        {
            var column = kvp.Key;
            var columnItem = kvp.Value;
            
            if (column == null || columnItem?.Settings == null) 
                continue;
            
            string mappingName = column.MappingName;
            string headerText = column.HeaderText;

            if (rowDict.TryGetValue(mappingName, out var cellValue))
            {
                
                var validationError = ValidateCellByDataType(columnItem, cellValue, headerText);

                if (validationError != null)
                {
                    result.IsValid = false;
                    result.ErrorMessages[mappingName] = validationError;
                }
                
                if (columnItem.Settings.IsUnique && !IsValueUnique(mappingName, cellValue, rowDict))
                {
                    result.IsValid = false;
                    result.ErrorMessages[mappingName] = $"Значення для '{headerText}' має бути унікальним";
                }
            }
            else if (columnItem.Settings.IsRequired)
            {
                result.IsValid = false;
                result.ErrorMessages[mappingName] = $"Значення для '{headerText}' обов'язкове для заповнення";
            }
            
        }

        return result;
    }

    private string? ValidateCellByDataType(ColumnItem columnItem, object cellValue, string headerText)
    {
        if (string.IsNullOrWhiteSpace(cellValue.ToString()))
        {
            return null;
        }

        return columnItem.Settings.DataType switch
        {
            ColumnDataType.Text => ValidateTextColumn(columnItem, cellValue, headerText),
            ColumnDataType.Number => ValidateNumberColumn(columnItem, cellValue, headerText),
            _ => null
        };
    }

    private string? ValidateTextColumn(ColumnItem columnItem, object cellValue, string headerText)
    {
        string? strValue = cellValue?.ToString();
        var textSettings = columnItem.Settings.SpecificSettings as TextColumnSettings;

        if (textSettings.MaxLength > 0 && strValue.Length > textSettings.MaxLength)
        {
            return
                $"Значення для '{headerText}' не може бути довше {TextLengthMessage((int)textSettings.MaxLength)} \n" +
                   $"Поточна довжина {TextLengthMessage(strValue.Length)}";
        }

        if (textSettings.MinLength > 0 && strValue.Length < textSettings.MinLength)
        {
            return $"Значення для '{headerText}' не може бути коротше {TextLengthMessage((int)textSettings.MinLength)} \n" +
                   $"Поточна довжина {TextLengthMessage(strValue.Length)}";
        }
        return null;
    }

    private string? ValidateTextMultilineColumn(ColumnItem columnItem, object cellValue, string headerText)
    {
        string? strValue = cellValue?.ToString();
        var multilineTextSettings = columnItem.Settings.SpecificSettings as MultilineTextColumnSettings;

        if (multilineTextSettings.MaxLength > 0 && strValue.Length > multilineTextSettings.MaxLength)
        {
            return $"Значення для '{headerText}' не може бути довше {TextLengthMessage((int)multilineTextSettings.MaxLength)}" +
                   $"Поточна довжина {TextLengthMessage(strValue.Length)}";
        }
        return null;
    }

    private string ValidateNumberColumn(ColumnItem columnItem, object cellValue, string headerText)
    {
        if (cellValue == null) return null;
        
        if (!double.TryParse(cellValue.ToString(), out double doubleValue))
        {
            return $"Значення для '{headerText}' має бути числом";
        }
        var numberSettings = columnItem.Settings.SpecificSettings as NumberColumnSettings;
        if (columnItem.Settings.IsRequired && doubleValue == 0)
        {
            return $"'{headerText}' є обов'язковим для заповнення";
        }
        if (numberSettings.MinValue > 0 && doubleValue < numberSettings.MinValue)
        {
            return $"Значення для '{headerText}' не може бути меншим за {numberSettings.MinValue}";
        }
        if (numberSettings.MaxValue > 0 && doubleValue > numberSettings.MaxValue)
        {
            return $"Значення для '{headerText}' не може бути більшим за {numberSettings.MaxValue}";
        }
        return null;
    }
    
    private bool IsValueUnique(string mappingName, object? value, IDictionary<string, object> currentRow)
    {
        foreach (var row in _rows)
        {
            if (row is IDictionary<string, object> rowDict)
            {
                if (ReferenceEquals(rowDict, currentRow))
                    continue; 

                if (rowDict.TryGetValue(mappingName, out var existingValue))
                {
                    if (Equals(existingValue, value))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    
    private string TextLengthMessage(int length)
    {
        return PluralizedHelper.GetPluralizedText(length, "символ", "символи", "символів");
    }
}