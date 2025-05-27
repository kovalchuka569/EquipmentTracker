using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;


namespace EquipmentTracker.ViewModels.DataGrid;

public class SizeChangedToViewModelBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
        nameof(Width), typeof(double), typeof(SizeChangedToViewModelBehavior), new PropertyMetadata(0d));

    public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
        nameof(Height), typeof(double), typeof(SizeChangedToViewModelBehavior), new PropertyMetadata(0d));

    public double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.SizeChanged -= AssociatedObject_SizeChanged;
        base.OnDetaching();
    }

    private void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Width = e.NewSize.Width;
        Height = e.NewSize.Height;
    }
}