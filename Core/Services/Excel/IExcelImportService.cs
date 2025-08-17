using Models.Common.Table;
using Models.Services;

namespace Core.Services.Excel;

public interface IExcelImportService
{
    
    Task<List<RowModel>> ImportRowsAsync(ExcelImportConfig importConfig);
}