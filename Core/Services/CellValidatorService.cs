using Core.Interfaces;
using Core.Models;
using Models.Common.Table.ColumnProperties;
using Models.Equipment;

namespace Core.Services;

public class CellValidatorService : ICellValidatorService
{
    public CellValidationResult ValidateCell(object? cellValue, object currentRow, List<string?> columnValues, ColumnDataType columnDataType, string headerText, BaseColumnProperties columnProperties)
    {
        var result = new CellValidationResult();
    
        var stringValue = cellValue switch
        {
            null => string.Empty,
            string str => str,
            _ => cellValue.ToString() ?? string.Empty
        };
    
        // Validate on required
        if (columnProperties.IsRequired && string.IsNullOrWhiteSpace(stringValue))
        {
            result.IsValid = false;
            result.ErrorMessage = $"Значення для '{headerText}' обов'язкове для заповнення.";
            return result;
        }
        

        // Validate on unique
        if (columnProperties.IsUnique)
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
            case ColumnDataType.Number:
                if(columnProperties is not NumberColumnProperties numberProps)
                    return result;

                if (!double.TryParse(stringValue, out double numericValue))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Значення для '{headerText}' може бути лише числовим";
                    return result;
                }
                
                if (numericValue < numberProps.MinNumberValue)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Значення для '{headerText}' не може бути меншим за {numberProps.MinNumberValue}.";
                    return result;
                }
                if (numericValue > numberProps.MaxNumberValue)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Значення для '{headerText}' не може бути більшим за {numberProps.MaxNumberValue}.";
                    return result;
                }
                break;
            case ColumnDataType.Text:
            case ColumnDataType.Date:
            case ColumnDataType.List:
            case ColumnDataType.Hyperlink:
            case ColumnDataType.Currency:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(columnDataType), columnDataType, null);
        }

        return result;
    }
}