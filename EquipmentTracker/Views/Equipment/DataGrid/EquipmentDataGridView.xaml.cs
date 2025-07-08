using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using UI.ViewModels.Equipment.DataGrid;

namespace EquipmentTracker.Views.Equipment.DataGrid;

public partial class EquipmentDataGridView : UserControl
{
    public EquipmentDataGridView()
    {
        InitializeComponent();
        AddHandler(PreviewMouseWheelEvent,
            new MouseWheelEventHandler(UIElement_OnPreviewMouseWheel),
            handledEventsToo: true);
    }

    private double _zoom = 1.0;
    private readonly Dictionary<object, double> RowHeights = new();
    private double DefaultHeight = 30;
    private double DefaultHeaderHeight = 30;
    private double CustomHeaderHeight = 30;
    private void SfDataGrid_OnQueryRowHeight(object? sender, QueryRowHeightEventArgs e)
        {
            var dataGrid = (SfDataGrid)sender!;
            
            if (e.RowIndex > 0)
            {
                var rowData = dataGrid.GetRecordAtRowIndex(e.RowIndex);
                if (rowData != null && RowHeights.TryGetValue(rowData, out double height))
                {
                    e.Height = height;
                    e.Handled = true;
                }
                else
                {
                    e.Height = DefaultHeight;
                    e.Handled = true;
                }
            }
        }
    

    private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            _zoom += e.Delta > 0 ? 0.1 : -0.1;
            _zoom = Math.Max(0.2, Math.Min(3.0, _zoom));

            MainScaleTransform.ScaleX = _zoom;
            MainScaleTransform.ScaleY = _zoom;

            e.Handled = true;
        }
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (e.Delta > 0)
                {
                    ChangeHeaderHeight(+10);
                }
                else if (e.Delta < 0)
                {
                    ChangeHeaderHeight(-10);
                }
                e.Handled = true;
            }
            else
            {
                var item = dataGrid.SelectedItem;
                if (item == null) return;

                if (e.Delta > 0)
                {
                    ChangeRowHeight(item, +10);
                }
                else if (e.Delta < 0)
                {
                    ChangeRowHeight(item, -10);
                }
                e.Handled = true;
            }
        }
    }

    private void ChangeHeaderHeight(double delta)
    {
        CustomHeaderHeight = Math.Max(20, CustomHeaderHeight + delta);
        dataGrid.HeaderRowHeight = CustomHeaderHeight;
        dataGrid.InvalidateMeasure();
        dataGrid.InvalidateArrange();
        dataGrid.InvalidateVisual();
        dataGrid.UpdateLayout(); 
    }

    private void ChangeRowHeight(object item, double delta)
    {
        if (!RowHeights.TryGetValue(item, out double currentHeight))
            currentHeight = DefaultHeight;

        double newHeight = Math.Max(20, currentHeight + delta);
        RowHeights[item] = newHeight;

        int rowIndex = dataGrid.ResolveToRowIndex(item);
        var rowColumnIndex = new RowColumnIndex(rowIndex, 0);
        dataGrid.InvalidateRowHeight(rowIndex);
        dataGrid.ScrollInView(rowColumnIndex);
        dataGrid.InvalidateMeasure(); 
        dataGrid.InvalidateArrange();
        dataGrid.InvalidateVisual();
        dataGrid.UpdateLayout();
    }
}
    