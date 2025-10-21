using Presentation.Services.Interfaces;
using Syncfusion.UI.Xaml.Grid;

namespace Presentation.Interfaces;

public interface IExcelExportManager
{
    void ExportToExcel(SfDataGrid dataGrid, string fileName, ISnackbarService snackbarService);
}