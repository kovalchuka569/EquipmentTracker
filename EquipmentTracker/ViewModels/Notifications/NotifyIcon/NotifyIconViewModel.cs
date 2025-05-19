using System.Windows;
using Syncfusion.Windows.Tools.Controls;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;

namespace UI.ViewModels.Notifications.NoifyIcon;

public class NotifyIconViewModel : BindableBase
{
    private NotifyIcon _notifyIcon;
    public DelegateCommand ShowCommand { get; }
    public NotifyIconViewModel()
    {
        Console.WriteLine("NotifyIconViewModel: Конструктор ViewModel");
        ShowCommand = new DelegateCommand(ShowBalloonTip);
    }

    public void SetNotifyIcon(NotifyIcon notifyIcon)
    {
        Console.WriteLine("NotifyIconViewModel: SetNotifyIcon вызван");
        _notifyIcon = notifyIcon;
        if (_notifyIcon != null)
        {
            Console.WriteLine("NotifyIconViewModel: _notifyIcon успешно инициализирован");
        }
        else
        {
            Console.WriteLine("NotifyIconViewModel: _notifyIcon равен null");
        }
    }

    private void ShowBalloonTip()
    {
        Console.WriteLine("NotifyIconViewModel: ShowBalloonTip вызван");
        if (_notifyIcon == null)
        {
            Console.WriteLine("NotifyIconViewModel: _notifyIcon не инициализирован!");
            Console.WriteLine("NotifyIcon не инициализирован!");
            return;
        }

        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            Console.WriteLine("NotifyIconViewModel: Показ уведомления через Dispatcher");

            // Показ уведомления
            _notifyIcon.Visibility = Visibility.Visible;
            _notifyIcon.ShowBalloonTip(5000);
        }));
    }


}