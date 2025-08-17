using System.Windows;
using Core.Common.Enums;

namespace EquipmentTracker.Common.Controls;

public partial class DialogBox
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(DialogBoxIcon), typeof(DialogBox), new PropertyMetadata(DialogBoxIcon.None));

    public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(nameof(Buttons), typeof(DialogBoxButtons), typeof(DialogBox), new PropertyMetadata(DialogBoxButtons.None));

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(DialogBox), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(DialogBox), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty Button1TextProperty = DependencyProperty.Register(nameof(Button1Text), typeof(string), typeof(DialogBox), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty Button2TextProperty = DependencyProperty.Register(nameof(Button2Text), typeof(string), typeof(DialogBox), new PropertyMetadata(string.Empty));

    public DialogBoxIcon Icon
    {
        get => (DialogBoxIcon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public DialogBoxButtons Buttons
    {
        get => (DialogBoxButtons)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string Button1Text
    {
        get => (string)GetValue(Button1TextProperty);
        set => SetValue(Button1TextProperty, value);
    }
    
    public string Button2Text
    {
        get => (string)GetValue(Button2TextProperty);
        set => SetValue(Button2TextProperty, value);
    }
    
    public DialogBox()
    {
        InitializeComponent();
    }
}