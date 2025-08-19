using System;
using System.Windows;
using System.Windows.Documents;

using Notification.Wpf;

using Presentation.Interfaces;

namespace Presentation.UIManagers;

public class GridInteractionManager(NotificationManager notificationManager) : IGridInteractionManager
{
    
    public void OnHyperlinkCellClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Hyperlink hyperlink) return;
        if (hyperlink.Inlines.FirstInline is not Run run || string.IsNullOrEmpty(run.Text)) return;
        var uriText = run.Text.Trim();
        
        if (Uri.TryCreate(uriText, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps
                || uri.Scheme == Uri.UriSchemeFile))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                notificationManager.Show( "Помилка",$"Не вдалось відкрити посилання: {ex.Message}", NotificationType.Error);
            }
        }
        else
        {
            notificationManager.Show("Посилання некоректне і не може бути відкрите.", NotificationType.Information);
        }
    }
}