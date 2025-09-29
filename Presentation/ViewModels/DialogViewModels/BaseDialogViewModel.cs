using System;
using Common.Logging;
using Presentation.Contracts;
using Notification.Wpf;
using Prism.Commands;
using Prism.Dialogs;

namespace Presentation.ViewModels.DialogViewModels;

public abstract class BaseDialogViewModel<T> : BaseViewModel<T>, IDialogAware, IClosableDialog
{
    protected BaseDialogViewModel(NotificationManager notificationManager, IAppLogger<T> logger) 
        : base(notificationManager, logger)
    {
    }

    private DelegateCommand<IDialogResult>? _closeDialogFromHostCommand;

    public bool CanCloseDialog() => true;

    public virtual void OnDialogClosed()
    {
        var result = new DialogResult
        {
            Result = ButtonResult.Cancel
        };
        _closeDialogFromHostCommand?.Execute(result);
    }
    
    protected void OnDialogClosed(DialogParameters parameters)
    {
        var dialogResult = new DialogResult
        {
            Parameters = parameters
        };
        
        OnDialogClosed(dialogResult);
    }

    protected void OnDialogClosed(IDialogResult dialogResult)
    {
        _closeDialogFromHostCommand?.Execute(dialogResult);
    }

    public virtual void OnDialogOpened(IDialogParameters parameters) { }

    public DialogCloseListener RequestClose { get; } = new();

    public virtual void SetCloseCommand(DelegateCommand<IDialogResult>? closeCommand)
    {
        _closeDialogFromHostCommand = closeCommand;
    }
}