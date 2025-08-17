using Core.Interfaces;
using Core.Common.Enums;
using Core.Models;

namespace EquipmentTracker.ViewModels.Dialogs;

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
    
    #region Interface implementation
    
    private DelegateCommand<IDialogResult>? _closeDialogFromHostCommand;
    
    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
        var result = new DialogResult
        {
            Parameters = new DialogParameters
            {
                {
                    "DialogBoxResult", DialogBoxResult.None
                }
            }
        };
        _closeDialogFromHostCommand?.Execute(result);
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
    }

    public DialogCloseListener RequestClose { get; } = new();
    public void SetCloseCommand(DelegateCommand<IDialogResult>? closeCommand)
    {
        _closeDialogFromHostCommand = closeCommand;
    }
    
    #endregion
}