using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.TextInputLayout;
using Syncfusion.Windows.Tools.Controls;

namespace Common.Behaviors;

public class ShowHidePasswordBehavior : Behavior<ButtonAdv>
{
    
    #region Dependency properties
    
    public static readonly DependencyProperty PasswordBoxProperty = DependencyProperty.Register(nameof(PasswordBox), typeof(PasswordBox), typeof(ShowHidePasswordBehavior));
    public static readonly DependencyProperty TextInputLayoutProperty = DependencyProperty.Register(nameof(TextInputLayout), typeof(SfTextInputLayout), typeof(ShowHidePasswordBehavior));
    public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(nameof(ShowIcon), typeof(ImageSource), typeof(ShowHidePasswordBehavior));
    public static readonly DependencyProperty HideIconProperty = DependencyProperty.Register(nameof(HideIcon), typeof(ImageSource), typeof(ShowHidePasswordBehavior));
    public static readonly DependencyProperty PasswordBoxTextProperty = DependencyProperty.Register(nameof(PasswordBoxText), typeof(string), typeof(ShowHidePasswordBehavior), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordBoxTextChanged));

    #endregion
    
    #region Private fields
    
    private TextBox? _textBox;
    private bool _isPasswordVisible;
    
    #endregion

    #region Public fields
    
    public PasswordBox? PasswordBox
    {
        get => (PasswordBox?)GetValue(PasswordBoxProperty);
        set => SetValue(PasswordBoxProperty, value);
    }

    public SfTextInputLayout? TextInputLayout
    {
        get => (SfTextInputLayout?)GetValue(TextInputLayoutProperty);
        set => SetValue(TextInputLayoutProperty, value);
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

    public string PasswordBoxText
    {
        get => (string)GetValue(PasswordBoxTextProperty);
        set => SetValue(PasswordBoxTextProperty, value);
    }
    
    #endregion
    
    #region Private methods

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Click += ShowHideButton_Click;
        
        if (PasswordBox != null) 
            PasswordBox.PasswordChanged += PasswordBoxOnPasswordChanged;

        CreateTextBox();

        UpdatePasswordVisibility();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Click -= ShowHideButton_Click;
        
        if (PasswordBox != null) 
            PasswordBox.PasswordChanged -= PasswordBoxOnPasswordChanged;
    }
    
    private void PasswordBoxOnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if(sender is not PasswordBox passwordBox)
            return;
        
        PasswordBoxText = passwordBox.Password;
    }
    
    private static void OnPasswordBoxTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ShowHidePasswordBehavior behavior) return;
        if (behavior.PasswordBox == null) return;

        string newPassword = e.NewValue?.ToString() ?? string.Empty;
        
        if (behavior.PasswordBox.Password != newPassword)
            behavior.PasswordBox.Password = newPassword;
    }

    private void CreateTextBox()
    {
        if (PasswordBox == null || TextInputLayout == null) return;

        _textBox = new TextBox
        {
            Width = PasswordBox.Width,
            Visibility = Visibility.Collapsed,
            FontFamily = PasswordBox.FontFamily,
            FontSize = PasswordBox.FontSize
        };

        _textBox.TextChanged += (_, _) =>
        {
            if (PasswordBox != null)
                PasswordBox.Password = _textBox?.Text ?? string.Empty;
        };

        var parent = PasswordBox.Parent as Panel;
        parent?.Children.Add(_textBox);
    }

    private void ShowHideButton_Click(object sender, RoutedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        
        UpdatePasswordVisibility();
    }

    private void UpdatePasswordVisibility()
    {
        if (PasswordBox == null || _textBox == null || TextInputLayout == null) return;

        if (_isPasswordVisible)
        {
            _textBox.Text = PasswordBox.Password;
            _textBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
            TextInputLayout.InputView = _textBox;
            AssociatedObject.SmallIcon = ShowIcon;
        }
        else
        {
            PasswordBox.Visibility = Visibility.Visible;
            _textBox.Visibility = Visibility.Collapsed;
            TextInputLayout.InputView = PasswordBox;
            AssociatedObject.SmallIcon = HideIcon;
        }
    }
    
    #endregion
}