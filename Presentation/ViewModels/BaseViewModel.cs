using System;
using System.Threading.Tasks;

using Prism.Mvvm;

using Notification.Wpf;

using Common.Logging;

namespace Presentation.ViewModels;

public abstract class BaseViewModel<TSelf> : BindableBase
{
    protected readonly NotificationManager NotificationManager;
    protected readonly IAppLogger<TSelf> Logger;

    protected BaseViewModel(NotificationManager notificationManager, IAppLogger<TSelf> logger)
    {
        NotificationManager = notificationManager;
        Logger = logger;
    }

    protected async Task ExecuteWithErrorHandlingAsync(Func<Task> action, 
        string uiMessage, 
        string logMessage, 
        Action<Exception>? onError = null,
        Action? onFinally = null)
    {
        try
        {
            await action();
        }
        catch (Exception e)
        {
            NotificationManager.Show(uiMessage);
            Logger.LogError(e, logMessage);
            onError?.Invoke(e);
        }
        finally
        {
            onFinally?.Invoke();
        }
    }
}