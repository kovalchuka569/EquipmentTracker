using Core.Interfaces;
using Core.Models;

namespace Core.Services;

public class RowValidatorService(ICellValidatorService cellValidatorService) : IRowValidatorService
{
    /// <summary>
    /// The loop calls validation for each cell in the provided row
    /// </summary>
    /// <param name="args">Row validation args</param>
    /// <returns>Row validation result</returns>
    /// <exception cref="InvalidOperationException">If row is not ExpandoObject</exception>
    public RowValidationResult ValidateRow(RowValidationArgs args)
    {
        var result = new RowValidationResult();
        

        foreach (var mappingPair in args.ColumnMappingNameIdMap)
        {
            var mappingName = mappingPair.Key;
            var columnId = mappingPair.Value;
            var headerText = args.ColumnIdHeaderTextMap[columnId];
            
            var columnDataType = args.ColumnIdDataTypeMap[columnId];
            var columnProps = args.ColumnIdPropertiesMap[columnId];
            
            var cellValue = args.CurrentRow.Cells
                .FirstOrDefault(c => c.ColumnMappingName == mappingName)
                ?.Value;
            
            var currentId = args.CurrentRow.Id;
            
            var columnValues = args.Rows
                .Where(row => row.Id != currentId)                       
                .Select(row => row.Cells
                    .FirstOrDefault(c => c.ColumnMappingName == mappingName)?
                    .Value?.ToString()?.Trim())                          
                .Where(s => !string.IsNullOrEmpty(s))                         
                .ToList();
            
            var cellResult = cellValidatorService.ValidateCell(cellValue, args.CurrentRow, columnValues, columnDataType, headerText, columnProps);

            if (cellResult.IsValid) continue;
            result.IsValid = false;
            result.ErrorMessages[mappingName] = cellResult.ErrorMessage ?? "Помилка";
        }
        return result;
    }
}