using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace Presentation.Controls;

public partial class Overlay : UserControl
{
    public static readonly DependencyProperty OverlayColorProperty = DependencyProperty.Register(nameof(OverlayColor), typeof(Brush), typeof(Overlay), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

    public static readonly DependencyProperty OverlayOpacityProperty = DependencyProperty.Register(nameof(OverlayOpacity), typeof(double), typeof(Overlay), new PropertyMetadata(0.5));

    public Brush OverlayColor
    {
        get => (Brush)GetValue(OverlayColorProperty);
        set => SetValue(OverlayColorProperty, value);
    }

    public double OverlayOpacity
    {
        get => (double)GetValue(OverlayOpacityProperty);
        set => SetValue(OverlayOpacityProperty, value);
    }
    
    public Overlay()
    {
        InitializeComponent();
    }
    
}