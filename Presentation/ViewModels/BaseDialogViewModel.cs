using System;
using Common.Logging;
using Core.Interfaces;
using Notification.Wpf;
using Prism.Commands;
using Prism.Dialogs;

namespace Presentation.ViewModels;

public abstract class BaseDialogViewModel<T> : BaseViewModel<T>, IDialogAware, IClosableDialog
{
    protected BaseDialogViewModel(NotificationManager notificationManager, IAppLogger<T> logger) 
        : base(notificationManager, logger)
    {
    }

    private DelegateCommand<IDialogResult>? _closeDialogFromHostCommand;

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
        Console.WriteLine("OnDialogClosed");
        var result = new DialogResult
        {
            Result = ButtonResult.Cancel
        };
        _closeDialogFromHostCommand?.Execute(result);
    }

    public void OnDialogOpened(IDialogParameters parameters) { }

    public DialogCloseListener RequestClose { get; } = new();

    public void SetCloseCommand(DelegateCommand<IDialogResult>? closeCommand)
    {
        _closeDialogFromHostCommand = closeCommand;
    }
}