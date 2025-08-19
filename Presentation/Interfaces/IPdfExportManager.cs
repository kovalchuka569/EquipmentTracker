using Notification.Wpf;

using Syncfusion.UI.Xaml.Grid;

namespace Presentation.Interfaces;

public interface IPdfExportManager
{
    void ExportToPdf(SfDataGrid dataGrid, string fileName, NotificationManager notificationManager);
}