using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.Office2019Colorful.WPF;
using Syncfusion.UI.Xaml.CellGrid;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid.ScrollAxis;
using RowColumnIndex = Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex;

namespace EquipmentTracker.Views.Equipment.DataGrid;

public partial class ColumnCreatorView : UserControl
{
    public ColumnCreatorView()
    {
        InitializeComponent();
    }
}