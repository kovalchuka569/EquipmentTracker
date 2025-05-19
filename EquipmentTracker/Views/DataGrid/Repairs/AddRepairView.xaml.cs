using System.Windows.Controls;

namespace UI.Views.DataGrid.Repairs;

public partial class AddRepairView : UserControl
{
    public AddRepairView()
    {
        InitializeComponent();
        Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("uk-UA");
    }
}