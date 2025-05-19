using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Models.DataGrid;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using UI.ViewModels.DataGrid;
using RowColumnIndex = Syncfusion.Windows.Controls.Cells.RowColumnIndex;

namespace UI.Views;

public partial class DataGridView : UserControl
{
    public DataGridView()
    {
        InitializeComponent();
    }
}