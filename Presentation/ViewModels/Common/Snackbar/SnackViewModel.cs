using Common.Enums;
using Presentation.Models;

namespace Presentation.ViewModels.Common.Snackbar;

public class SnackViewModel : ViewModelBase
{
    private string _message = string.Empty;
    private SnackType _type;
    private bool _showed = true;
    private int _showTime;
    
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public SnackType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public bool Showed
    {
        get => _showed;
        set => SetProperty(ref _showed, value);
    }

    public int ShowTime
    {
        get => _showTime;
        set => SetProperty(ref _showTime, value);
    }

    public SnackViewModel FromDomain(Snack snack)
    {
        _message = snack.Message;
        _type = snack.Type;
        ShowTime = snack.ShowTime;
        return this;
    }
}