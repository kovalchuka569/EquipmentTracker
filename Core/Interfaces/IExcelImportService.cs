using Models.Common.Table;
using Models.Services;

namespace Core.Interfaces;

public interface IExcelImportService
{
    
    /// <summary>
    /// Imports rows from an Excel file based on the provided configuration.
    /// </summary>
    /// <param name="importConfig">The configuration specifying how to import the data.</param>
    /// <returns>A task that represents the asynchronous import operation, containing a list of imported RowModel objects.</returns>
    Task<List<RowModel>> ImportRowsAsync(ExcelImportConfig importConfig);
}