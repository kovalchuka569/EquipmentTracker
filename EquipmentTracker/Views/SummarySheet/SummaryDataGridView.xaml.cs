using System.Windows.Controls;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid;

namespace EquipmentTracker.Views.SummarySheet;

public partial class SummaryDataGridView : UserControl
{
    public SummaryDataGridView()
    {
        InitializeComponent();
        AddHandler(PreviewMouseWheelEvent,
            new MouseWheelEventHandler(UIElement_OnPreviewMouseWheel),
            handledEventsToo: true);
    }
    private double _zoom = 1.0;

    private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            _zoom += e.Delta > 0 ? 0.1 : -0.1;
            _zoom = Math.Max(0.2, Math.Min(3.0, _zoom));

           // MainScale.ScaleX = _zoom;
           // MainScaleTransform.ScaleY = _zoom;

            e.Handled = true;
        }
    }
    
}