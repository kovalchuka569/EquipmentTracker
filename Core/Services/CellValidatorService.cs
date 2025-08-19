using Core.Common.Helpers;
using Core.Interfaces;
using Core.Models;

using Models.Common.Table.ColumnValidationRules;
using Models.Equipment;

namespace Core.Services;

public class CellValidatorService : ICellValidatorService
{
    /// <summary>
    /// Validates a single cell value based on the provided validation rules.
    /// </summary>
    /// <param name="cellValue">Cell value for validation</param>
    /// <param name="currentRow">Current row</param>
    /// <param name="columnValues">Column values for unique validation</param>
    /// <param name="columnDataType">Column data type</param>
    /// <param name="headerText">Column header text</param>
    /// <param name="columnValidationRules">Aggregated validation rules for the column the cell belongs to</param>
    /// <returns>Validation result containing status and error message if invalid</returns>
    public CellValidationResult ValidateCell(object? cellValue, object currentRow, List<string?> columnValues, ColumnDataType columnDataType, string headerText, IColumnValidationRules columnValidationRules)
    {
        var result = new CellValidationResult();

        var stringValue = cellValue switch
        {
            null => string.Empty,
            string str => str,
            _ => cellValue.ToString() ?? string.Empty
        };
        
        // Validate on required
        if (columnValidationRules.IsRequired && string.IsNullOrWhiteSpace(stringValue))
        {
            result.IsValid = false;
            result.ErrorMessage = $"Значення для '{headerText}' обов'язкове для заповнення.";
            return result;
        }

        // Validate on unique
        if (columnValidationRules.IsUnique)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
                return result;

            if (columnValues.Contains(stringValue))
            {
                result.IsValid = false;
                result.ErrorMessage =
                    $"Значення для '{headerText}' повинно бути унікальним\n" +
                    $"{stringValue} знайдено в інших рядках";
                return result;
            }
        }
        
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return result;
        }
        
        switch (columnDataType)
        {
            case ColumnDataType.Text:

                if (columnValidationRules is not TextColumnValidationRules textColumnValidationRules) return result;
                
                if (stringValue.Length < textColumnValidationRules.MinLength)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Значення для '{headerText}' не може бути коротше {TextLengthMessage(textColumnValidationRules.MinLength)} \n" +
                                          $"Поточна довжина {TextLengthMessage(stringValue.Length)}";
                    return result;
                }
                
                if (stringValue.Length > textColumnValidationRules.MaxLength)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Значення для '{headerText}' не може бути довше {TextLengthMessage(textColumnValidationRules.MaxLength)} \n" +
                                          $"Поточна довжина {TextLengthMessage(stringValue.Length)}";
                    return result;
                }
                break;

            case ColumnDataType.Number:
                if (columnValidationRules is not NumericColumnValidationRules numericColumnValidationRules) return result;

                if (!double.TryParse(stringValue, out double numericValue))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Значення для '{headerText}' може бути лише числовим";
                    return result;
                }
                
                if (numericValue < numericColumnValidationRules.MinValue)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Значення для '{headerText}' не може бути меншим за {numericColumnValidationRules.MinValue}.";
                    return result;
                }
                if (numericValue > numericColumnValidationRules.MaxValue)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Значення для '{headerText}' не може бути більшим за {numericColumnValidationRules.MaxValue}.";
                    return result;
                }
                break;
            case ColumnDataType.Date:
            case ColumnDataType.Boolean:
            case ColumnDataType.List:
            case ColumnDataType.Hyperlink:
            case ColumnDataType.Currency:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(columnDataType), columnDataType, null);
        }

        return result;
    }
    
    private static string TextLengthMessage(int length)
    {
        return PluralizedHelper.GetPluralizedText(length, "символ", "символи", "символів");
    }
}