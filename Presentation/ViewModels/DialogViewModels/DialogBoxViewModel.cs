using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;

using Presentation.Contracts;
using JetBrains.Annotations;
using Presentation.Enums;
using Presentation.EventArgs;
using Presentation.Models;
    
namespace Presentation.ViewModels.DialogViewModels;

public class DialogBoxViewModel : BindableBase, IDialogAware, IClosableDialog
{
    #region UI Fields
    
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
    
    private DialogBoxIcon _icon = DialogBoxIcon.None;
    public DialogBoxIcon Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }
    
    private DialogBoxButtons _buttons = DialogBoxButtons.None;
    public DialogBoxButtons Buttons
    {
        get => _buttons;
        set => SetProperty(ref _buttons, value);
    }
    
    private string _button1Text = string.Empty;
    public string Button1Text
    {
        get => _button1Text;
        set => SetProperty(ref _button1Text, value);
    }
    
    private string _button2Text = string.Empty;
    public string Button2Text
    {
        get => _button2Text;
        set => SetProperty(ref _button2Text, value);
    }
    
    #endregion
    
    #region Commands implementation
    
    public DelegateCommand<DialogBoxResultEventArgs>? DialogResultSelectedCommand { [UsedImplicitly] get; private set; }

    private void InitializeCommands()
    {
        DialogResultSelectedCommand = new DelegateCommand<DialogBoxResultEventArgs>(OnDialogResultSelected);
    }
    
    #endregion
    
    #region Private methods
    
    private void OnDialogResultSelected(DialogBoxResultEventArgs result)
    {
        
        var dialogResult = new DialogResult
        {
            Parameters = new DialogParameters
            {
                {
                    "DialogBoxResult", result.DialogResult
                }
            }
        };
        _closeDialogFromHostCommand?.Execute(dialogResult);
    }
    
    #endregion
    
    #region Interfaces implementations
    
    private DelegateCommand<IDialogResult>? _closeDialogFromHostCommand;
    
    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
        var dialogResult = new DialogResult
        {
            Parameters = new DialogParameters
            {
                {
                    "DialogBoxResult", DialogBoxResult.None
                }
            }
        };
        _closeDialogFromHostCommand?.Execute(dialogResult);
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        var dialogBoxParameters = parameters.GetValue<DialogBoxParameters>("DialogBoxParameters");
        Title = dialogBoxParameters.Title;
        Message = dialogBoxParameters.Message;
        Icon = dialogBoxParameters.Icon;
        Buttons = dialogBoxParameters.Buttons;

        if (dialogBoxParameters.ButtonsText is null) return;
        
        Button1Text = dialogBoxParameters.ButtonsText[0];
        Button2Text = dialogBoxParameters.ButtonsText[1];
        
        InitializeCommands();
    }

    public DialogCloseListener RequestClose { get; } = new();
    public void SetCloseCommand(DelegateCommand<IDialogResult>? closeCommand)
    {
        _closeDialogFromHostCommand = closeCommand;
    }
    
    #endregion
}