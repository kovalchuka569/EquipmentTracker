using System;
using System.Collections.ObjectModel;
using Common.Enums;
using MaterialDesignThemes.Wpf;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.Common.Snackbar;
using Prism.Commands;

namespace Presentation.ViewModels;

public class SnackbarViewModel : ViewModelBase
{
    private ObservableCollection<SnackBarItemViewModel> _activeSnackbarItems = [];

    public ObservableCollection<SnackBarItemViewModel> ActiveSnackbarItems
    {
        get => _activeSnackbarItems;
        set => SetProperty(ref _activeSnackbarItems, value);
    }
    
    public DelegateCommand AddItemCommand { get; set; }
    public DelegateCommand<object> CloseSnackbarItemCommand { get; set; }

    public SnackbarViewModel()
    {
        AddItemCommand = new DelegateCommand(OnAddSnackbarItemExecuted);
        CloseSnackbarItemCommand = new DelegateCommand<object>(OnCloseSnackbarItemExecuted);
    }

    private void OnAddSnackbarItemExecuted()
    {
        Random rnd = new Random();
        
        var styles = Enum.GetValues(typeof(SnackbarStyle));
        
        var randomStyle = (SnackbarStyle)(styles.GetValue(rnd.Next(styles.Length)) ?? SnackbarStyle.None);
        ActiveSnackbarItems.Add(new SnackBarItemViewModel
        {
            Message = "Caution",
            SnackbarContainer = SnackbarContainer.Text,
            SnackbarStyle = randomStyle
        });
    }

    private void OnCloseSnackbarItemExecuted(object parameter)
    {
        var snackbarItem = (SnackBarItemViewModel) parameter;

        ActiveSnackbarItems.Remove(snackbarItem);
    }
}