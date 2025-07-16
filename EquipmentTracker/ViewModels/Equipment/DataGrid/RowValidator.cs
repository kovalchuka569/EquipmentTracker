using System.Text.RegularExpressions;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
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

    public RowValidator(Dictionary<GridColumnBase, ColumnItem> columnItemMap)
    {
        _columnItemMap = columnItemMap;
    }

    public ValidationResult ValidateRow(object rowData)
    {
        var result = new ValidationResult();

        if (rowData is not EquipmentRow equipmentRow)
        {
            return result;
        }
        
        var rowDict = equipmentRow.Data as IDictionary<string, object>;

        foreach (var kvp in _columnItemMap)
        {
            var column = kvp.Key;
            var columnItem = kvp.Value;
            
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
            }
            else if (columnItem.Settings.IsRequired)
            {
                result.IsValid = false;
                result.ErrorMessages[mappingName] = $"Поле '{headerText}' обов'язкове для заповнення";
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
            ColumnDataType.MultilineText => ValidateTextMultilineColumn(columnItem, cellValue, headerText),
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
            return $"Значення '{headerText}' не може бути довше {textSettings.MaxLength} символів";
        }

        if (textSettings.MinLength > 0 && strValue.Length < textSettings.MinLength)
        {
            return $"Значення '{headerText}' не може бути коротше {textSettings.MinLength} символів";
        }
        if (!string.IsNullOrEmpty(textSettings.RegularExpressionPattern) &&
            !Regex.IsMatch(strValue, textSettings.RegularExpressionPattern))
        {
            string hint = GetRegexHint(textSettings.RegularExpressionPattern);
            return $"Значення не відповідає встановленому формату. {hint}";
        }
        return null;
    }

    private string? ValidateTextMultilineColumn(ColumnItem columnItem, object cellValue, string headerText)
    {
        string? strValue = cellValue?.ToString();
        var multilineTextSettings = columnItem.Settings.SpecificSettings as MultilineTextColumnSettings;

        if (multilineTextSettings.MaxLength > 0 && strValue.Length > multilineTextSettings.MaxLength)
        {
            return $"Значення '{headerText}' не може бути довше {multilineTextSettings.MaxLength} символів";
        }
        return null;
    }

    private string ValidateNumberColumn(ColumnItem columnItem, object cellValue, string headerText)
    {
        double doubleValue = Convert.ToDouble(cellValue);
        var numberSettings = columnItem.Settings.SpecificSettings as NumberColumnSettings;
        if (columnItem.Settings.IsRequired && doubleValue == 0)
        {
            return $"Поле '{headerText}' обов'язкове для заповнення";
        }
        if (numberSettings.MinValue > 0 && doubleValue < numberSettings.MinValue)
        {
            return $"Значення в полі '{headerText}' не може бути меншим за {numberSettings.MinValue}";
        }
        if (numberSettings.MaxValue > 0 && doubleValue > numberSettings.MaxValue)
        {
            return $"Значення в полі '{headerText}' не може бути більшим за {numberSettings.MaxValue}";
        }
        return null;
    }

    private string GetRegexHint(string regexPattern)
    {
        var regexInfo = ComboBoxRegularExpression.GetComboBoxRegularExpressions().FirstOrDefault(r => r.RegularExpressionPattern == regexPattern);
        if (regexInfo != null)
        {
            return $"{regexInfo.RegularExpressionTitle}. Приклад: {regexInfo.Example}";
        }
        return "Перевірте правильність введених даних";
    }
}