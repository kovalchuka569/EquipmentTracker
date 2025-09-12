using Models.Equipment;

using Core.Models;
using Models.Common.Table.ColumnProperties;

namespace Core.Interfaces;

public interface ICellValidatorService
{
    /// <summary>
    /// Validates a single cell value based on the provided validation rules.
    /// </summary>
    /// <param name="cellValue">Cell value for validation</param>
    /// <param name="currentRow">Current row</param>
    /// <param name="columnValues">Column values for unique validation</param>
    /// <param name="columnDataType">Column data type</param>
    /// <param name="headerText">Column header text</param>
    /// <param name="columnProperties">Domain column properties</param>
    /// <returns>Validation result containing status and error message if invalid</returns>
    CellValidationResult ValidateCell(object? cellValue, object currentRow, List<string?> columnValues,
        ColumnDataType columnDataType, string headerText, BaseColumnProperties columnProperties);
}