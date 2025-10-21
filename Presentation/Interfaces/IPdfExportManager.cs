using Presentation.Services.Interfaces;
using Syncfusion.UI.Xaml.Grid;

namespace Presentation.Interfaces;

public interface IPdfExportManager
{
    void ExportToPdf(SfDataGrid dataGrid, string fileName, ISnackbarService snackbarService);
}