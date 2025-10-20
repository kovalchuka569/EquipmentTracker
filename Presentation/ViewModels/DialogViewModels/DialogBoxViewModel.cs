using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common.Constants;
using Common.Enums;
using JetBrains.Annotations;
using Presentation.Models;
using Presentation.ViewModels.Common.DialogBox;
using Presentation.ViewModels.DialogViewModels.Common;
using Prism.Commands;
using Prism.Dialogs;

namespace Presentation.ViewModels.DialogViewModels;

public class DialogBoxViewModel : DialogViewModelBase
{
    private string _title = string.Empty;
    private string _message = string.Empty;
    private DialogBoxIcon _icon = DialogBoxIcon.None;
    private List<DialogBoxButtonInfo> _buttons = [];
    
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public DialogBoxIcon Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public List<DialogBoxButtonInfo> Buttons
    {
        get => _buttons;
        set
        {
            SetProperty(ref _buttons, value);
            UpdateVisibleButtons();
        }
    }
    
    [UsedImplicitly]
    public ObservableCollection<DialogBoxButtonViewModel> VisibleButtons { get; } = new();


    public DelegateCommand? Button1Command { get; private set; }
    public DelegateCommand? Button2Command { get; private set; }
    public DelegateCommand? Button3Command { get; private set; }
    
    public override void OnDialogOpened(IDialogParameters parameters)
    {
        Title = parameters.GetValue<string>(DialogBoxParameterKeys.TitleKey);
        Message = parameters.GetValue<string>(DialogBoxParameterKeys.MessageKey);
        Icon = parameters.GetValue<DialogBoxIcon>(DialogBoxParameterKeys.IconKey);
        Buttons = parameters.GetValue<List<DialogBoxButtonInfo>>(DialogBoxParameterKeys.ButtonsParametersKey);
    }
    
    private void UpdateCommands()
    {
        Button1Command = _buttons.Count > 0 
            ? new DelegateCommand(() => CloseWithButton(1, _buttons[0].Text))
            : null;
            
        Button2Command = _buttons.Count > 1 
            ? new DelegateCommand(() => CloseWithButton(2, _buttons[1].Text))
            : null;
            
        Button3Command = _buttons.Count > 2 
            ? new DelegateCommand(() => CloseWithButton(3, _buttons[2].Text))
            : null;

        RaisePropertyChanged(nameof(Button1Command));
        RaisePropertyChanged(nameof(Button2Command));
        RaisePropertyChanged(nameof(Button3Command));
    }
    
    private void UpdateVisibleButtons()
    {
        UpdateCommands();
        VisibleButtons.Clear();
        
        if (_buttons.Count > 0 && Button1Command != null)
            VisibleButtons.Add(new DialogBoxButtonViewModel
            { 
                Text = _buttons[0].Text, 
                Style = _buttons[0].Style, 
                Command = Button1Command 
            });
            
        if (_buttons.Count > 1 && Button2Command != null)
            VisibleButtons.Add(new DialogBoxButtonViewModel 
            { 
                Text = _buttons[1].Text, 
                Style = _buttons[1].Style, 
                Command = Button2Command 
            });
            
        if (_buttons.Count > 2 && Button3Command != null)
            VisibleButtons.Add(new DialogBoxButtonViewModel 
            { 
                Text = _buttons[2].Text, 
                Style = _buttons[2].Style, 
                Command = Button3Command 
            });
    }
    
    private void CloseWithButton(int buttonId, string buttonText) => 
        OnDialogClosed(new DialogParameters 
        { 
            { DialogBoxParameterKeys.ClickedButtonKey, buttonId },
            { DialogBoxParameterKeys.ClickedButtonTextKey, buttonText }
        });
}