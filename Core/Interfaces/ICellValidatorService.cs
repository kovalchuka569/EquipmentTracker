using Models.Common.Table.ColumnValidationRules;
using Models.Equipment;

using Core.Models;

namespace Core.Interfaces;

public interface ICellValidatorService
{
    CellValidationResult ValidateCell(object? cellValue, object currentRow, List<string?> columnValues,
        ColumnDataType columnDataType, string headerText, IColumnValidationRules columnValidationRules);
}