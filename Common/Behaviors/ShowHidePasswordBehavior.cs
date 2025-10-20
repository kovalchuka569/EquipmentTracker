using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.TextInputLayout;
using Syncfusion.Windows.Tools.Controls;

namespace Common.Behaviors;

public class ShowHidePasswordBehavior : Behavior<SfTextInputLayout>
{
    #region Dependency properties
    
    public static readonly DependencyProperty PasswordFontProperty=
        DependencyProperty.Register(nameof(PasswordFont), typeof(FontFamily), typeof(ShowHidePasswordBehavior));

    public static readonly DependencyProperty ShowHideButtonProperty =
        DependencyProperty.Register(nameof(ShowHideButton), typeof(ButtonAdv), typeof(ShowHidePasswordBehavior));

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(nameof(ShowIcon), typeof(ImageSource), typeof(ShowHidePasswordBehavior));

    public static readonly DependencyProperty HideIconProperty =
        DependencyProperty.Register(nameof(HideIcon), typeof(ImageSource), typeof(ShowHidePasswordBehavior));

    #endregion

    #region Private fields
    
    private bool _isPasswordVisible;
    private TextBox _textBox = new();
    private double _textBoxWidth;
    private double _textBoxHeight;
    private FontFamily _defaultFontFamily = new();

    #endregion

    #region Public fields

    public FontFamily? PasswordFont
    {
        get => (FontFamily?)GetValue(PasswordFontProperty);
        set => SetValue(PasswordFontProperty, value);
    }

    public ButtonAdv? ShowHideButton
    {
        get => (ButtonAdv?)GetValue(ShowHideButtonProperty);
        set => SetValue(ShowHideButtonProperty, value);
    }

    public ImageSource? ShowIcon
    {
        get => (ImageSource?)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public ImageSource? HideIcon
    {
        get => (ImageSource?)GetValue(HideIconProperty);
        set => SetValue(HideIconProperty, value);
    }

    #endregion

    #region Private methods

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (ShowHideButton != null)
            ShowHideButton.Click += ShowHideButton_Click;
        
        if(AssociatedObject.InputView is TextBox textBox)
        {
            _textBox = textBox;
            _defaultFontFamily = textBox.FontFamily;
            _textBoxWidth = textBox.ActualWidth;
            _textBoxHeight = textBox.ActualHeight;
        }
        
        UpdatePasswordVisibility(); 
        UpdateIcon();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        
        AssociatedObject.Loaded -= OnLoaded;

        if (ShowHideButton != null)
            ShowHideButton.Click -= ShowHideButton_Click;
    }
    

    private void ShowHideButton_Click(object sender, RoutedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;

        UpdatePasswordVisibility();
        UpdateIcon();
    }

    private void UpdatePasswordVisibility()
    {
        _textBox.FontFamily = _isPasswordVisible ? _defaultFontFamily : PasswordFont;
        _textBox.Width = _textBoxWidth;
        _textBox.Height = _textBoxHeight;
    }
    
    private void UpdateIcon()
    {
        if (ShowHideButton != null && ShowIcon != null && HideIcon != null)
            ShowHideButton.SmallIcon = _isPasswordVisible ? HideIcon : ShowIcon;
    }

    #endregion
}