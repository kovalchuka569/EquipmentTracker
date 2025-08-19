using Models.Common.Table;
using Models.Services;

namespace Core.Interfaces;

public interface IExcelImportService
{
    Task<List<RowModel>> ImportRowsAsync(ExcelImportConfig importConfig);
}