using System.Windows;
using System.Windows.Input;

namespace Presentation.Views.DialogViews;

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