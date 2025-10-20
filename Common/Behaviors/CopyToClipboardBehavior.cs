using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;
using Syncfusion.Windows.Tools.Controls;

namespace Common.Behaviors;

public class CopyToClipboardBehavior : Behavior<ButtonAdv>
{
    public static readonly DependencyProperty CopiedTextProperty =
        DependencyProperty.Register(nameof(CopiedText), typeof(string), typeof(CopyToClipboardBehavior));

    public string CopiedText
    {
        get => (string)GetValue(CopiedTextProperty);
        set => SetValue(CopiedTextProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is ButtonBase button) button.Click += Button_OnClick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is ButtonBase button)
            button.Click -= Button_OnClick;
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(CopiedText);
    }
}