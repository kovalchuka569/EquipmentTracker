using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;
using Syncfusion.Windows.Tools.Controls;

namespace Common.Behaviors;

public class SfButtonAdvPopupBehavior : Behavior<ButtonAdv>
{
    public static readonly DependencyProperty PopupProperty = DependencyProperty.Register(nameof(Popup), typeof(Popup), typeof(SfButtonAdvPopupBehavior));
    
    public Popup Popup
    {
        get => (Popup)GetValue(PopupProperty);
        set => SetValue(PopupProperty, value);
    }
    
    protected override void OnAttached()
    {
        base.OnAttached();
            
        if (AssociatedObject is ButtonBase button)
        {
            button.Click += OnButtonClick;
        }
    }
    
    protected override void OnDetaching()
    {
        if (AssociatedObject is ButtonBase button)
        {
            button.Click -= OnButtonClick;
        }
        base.OnDetaching();
    }
    
    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        Popup.SetCurrentValue(Popup.IsOpenProperty, !Popup.IsOpen);
    }
}