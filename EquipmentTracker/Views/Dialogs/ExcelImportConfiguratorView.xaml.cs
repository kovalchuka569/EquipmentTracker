using System.Windows;
using System.Windows.Input;

namespace EquipmentTracker.Views.Dialogs;

public partial class ExcelImportConfiguratorView
{
    public ExcelImportConfiguratorView()
    {
        InitializeComponent();
    }

    private void ExcelImportConfiguratorView_OnLoaded(object sender, RoutedEventArgs e)
    {
        Focusable = true;
        Keyboard.Focus(this);
    }
}