using Common.Enums;

namespace Presentation.ViewModels.Common.Snackbar;

public class SnackBarItemViewModel : ViewModelBase
{
    private string _message = string.Empty;
    private SnackbarContainer _snackbarContainer;
    private SnackbarStyle _snackbarStyle;

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public SnackbarContainer SnackbarContainer
    {
        get => _snackbarContainer;
        set => SetProperty(ref _snackbarContainer, value);
    }

    public SnackbarStyle SnackbarStyle
    {
        get => _snackbarStyle;
        set => SetProperty(ref _snackbarStyle, value);
    }
}