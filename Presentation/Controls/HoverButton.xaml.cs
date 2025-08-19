using System.Windows;
using System.Windows.Media;

namespace Presentation.Controls;

public partial class HoverButton
{
    #region Dependency properties
    
    public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register(nameof(ButtonWidth), typeof(double), typeof(HoverButton), new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty ButtonHeightProperty = DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(HoverButton), new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty ButtonCornerRadiusProperty = DependencyProperty.Register(nameof(ButtonCornerRadius), typeof(CornerRadius), typeof(HoverButton), new PropertyMetadata(new CornerRadius(3)));
    
    public static readonly DependencyProperty ContentPaddingProperty = DependencyProperty.Register(nameof(ContentPadding), typeof(Thickness), typeof(HoverButton), new PropertyMetadata(new Thickness(3)));
    
    public static readonly DependencyProperty NormalBackgroundProperty = DependencyProperty.Register(nameof(NormalBackground), typeof(Brush), typeof(HoverButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xE8))));

    public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(nameof(HoverBackground), typeof(Brush), typeof(HoverButton), new PropertyMetadata(Brushes.Gray));

    public static readonly DependencyProperty ContentFillProperty = DependencyProperty.Register(nameof(ContentFill), typeof(Brush), typeof(HoverButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x09, 0x24, 0x4B))));

    public static readonly DependencyProperty HoverContentFillProperty = DependencyProperty.Register(nameof(HoverContentFill), typeof(Brush), typeof(HoverButton), new PropertyMetadata(Brushes.White));

    public static readonly DependencyProperty NormalIconContentProperty = DependencyProperty.Register(nameof(NormalIconContent), typeof(ImageSource), typeof(HoverButton), new PropertyMetadata(null));
   
    public static readonly DependencyProperty NormalIconHeightProperty = DependencyProperty.Register(nameof(NormalIconHeight), typeof(double), typeof(HoverButton), new PropertyMetadata(double.NaN));
    
    public static readonly DependencyProperty NormalIconWidthProperty = DependencyProperty.Register(nameof(NormalIconWidth), typeof(double), typeof(HoverButton), new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty HoverIconWidthProperty = DependencyProperty.Register(nameof(HoverIconWidth), typeof(double), typeof(HoverButton), new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty HoverIconHeightProperty = DependencyProperty.Register(nameof(HoverIconHeight), typeof(double), typeof(HoverButton), new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty HoverIconContentProperty = DependencyProperty.Register(nameof(HoverIconContent), typeof(ImageSource), typeof(HoverButton), new PropertyMetadata(null));
    
    public static readonly DependencyProperty ChangeIconHoverProperty = DependencyProperty.Register(nameof(ChangeIconHover), typeof(bool), typeof(HoverButton), new PropertyMetadata(false));
    
    public static readonly DependencyProperty TextContentProperty = DependencyProperty.Register(nameof(TextContent), typeof(string), typeof(HoverButton), new PropertyMetadata(string.Empty));
    
    public static readonly DependencyProperty TextFontSizeProperty = DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(HoverButton), new PropertyMetadata(12.00));

    public static readonly DependencyProperty TextFontFamilyProperty = DependencyProperty.Register(nameof(TextFontFamilyProperty), typeof(FontFamily), typeof(HoverButton), new PropertyMetadata(new FontFamily("Microsoft Sans Serif")));
    
    #endregion
    
    #region Constructor
    
    public HoverButton()
    {
        InitializeComponent();
    }
    
    #endregion
    
    #region Fields
    
    public double ButtonWidth
    {
        get => (double)GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }
    
    public double ButtonHeight
    {
        get => (double)GetValue(ButtonHeightProperty);
        set => SetValue(ButtonHeightProperty, value);
    }

    public CornerRadius ButtonCornerRadius
    {
        get => (CornerRadius)GetValue(ButtonCornerRadiusProperty);
        set => SetValue(ButtonCornerRadiusProperty, value);
    }

    public Thickness ContentPadding
    {
        get => (Thickness)GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    
    public Brush NormalBackground
    {
        get => (Brush)GetValue(NormalBackgroundProperty);
        set => SetValue(NormalBackgroundProperty, value);
    }
    
    public Brush HoverBackground
    {
        get => (Brush)GetValue(HoverBackgroundProperty);
        set => SetValue(HoverBackgroundProperty, value);
    }
    
    public Brush ContentFill
    {
        get => (Brush)GetValue(ContentFillProperty);
        set => SetValue(ContentFillProperty, value);
    }
    
    public Brush HoverContentFill
    {
        get => (Brush)GetValue(HoverContentFillProperty);
        set => SetValue(HoverContentFillProperty, value);
    }
    
    public ImageSource? NormalIconContent
    {
        get => (ImageSource?)GetValue(NormalIconContentProperty);
        set => SetValue(NormalIconContentProperty, value);
    }

    public double NormalIconHeight
    {
        get => (double)GetValue(NormalIconHeightProperty);
        set => SetValue(NormalIconHeightProperty, value);
    }

    public double NormalIconWidth
    {
        get => (double)GetValue(NormalIconWidthProperty);
        set => SetValue(NormalIconWidthProperty, value);
    }

    public double HoverIconHeight
    {
        get => (double)GetValue(HoverIconHeightProperty);
        set => SetValue(HoverIconHeightProperty, value);
    }

    public double HoverIconWidth
    {
        get => (double)GetValue(HoverIconWidthProperty);
        set => SetValue(HoverIconWidthProperty, value);
    }
    
    public ImageSource? HoverIconContent
    {
        get => (ImageSource?)GetValue(HoverIconContentProperty);
        set => SetValue(HoverIconContentProperty, value);
    }

    public bool ChangeIconHover
    {
        get => (bool)GetValue(ChangeIconHoverProperty);
        set => SetValue(ChangeIconHoverProperty, value);
    }

    public string TextContent
    {
        get => (string)GetValue(TextContentProperty);
        set => SetValue(TextContentProperty, value);
    }

    public double TextFontSize
    {
        get => (double)GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    public FontFamily TextFontFamily
    {
        get => (FontFamily)GetValue(TextFontFamilyProperty);
        set => SetValue(TextFontFamilyProperty, value);
    }
    
    #endregion

    #region Private methods
    
    private void HoverButton_OnLoaded(object sender, RoutedEventArgs e)
    {
        NormalIcon.Source = NormalIconContent;
        HoverIcon.Source = HoverIconContent;
        
        UpdateContent();
        
        BorderElement.MouseEnter += (_, _) => { UpdateContent(true); };
        BorderElement.MouseLeave += (_, _) => { UpdateContent(); };
    }
    
    private void UpdateContent(bool isHovered = false)
    {
        if (ChangeIconHover)
        {
            NormalIcon.Visibility = isHovered ? Visibility.Collapsed : Visibility.Visible;
            HoverIcon.Visibility = isHovered ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            NormalIcon.Visibility = Visibility.Visible;
            HoverIcon.Visibility = Visibility.Collapsed;
        }
        
        TextPresenter.Foreground = isHovered ? HoverContentFill : ContentFill;
    }
    
    #endregion
}