using Presentation.Contracts;
using Presentation.ViewModels.Common;
using Prism.Commands;
using Prism.Dialogs;

namespace Presentation.ViewModels.DialogViewModels.Common;

public abstract class DialogViewModelBase
    : ViewModelBase, IDialogAware, IClosableDialog
{

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