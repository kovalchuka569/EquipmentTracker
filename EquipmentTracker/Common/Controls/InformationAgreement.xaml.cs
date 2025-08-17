using System.Windows;
using Syncfusion.Windows.Shared;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace EquipmentTracker.Common.Controls;

public partial class InformationAgreement : ChromelessWindow
{
    public static readonly DependencyProperty InformationTitleProperty = DependencyProperty.Register(nameof(InformationTitle), typeof(string), typeof(InformationAgreement));
        
    public static readonly DependencyProperty InformationMessageProperty = DependencyProperty.Register(nameof(InformationMessage), typeof(string), typeof(InformationAgreement));
    
    public static readonly DependencyProperty CancelButtonVisibilityProperty = DependencyProperty.Register(nameof(CancelButtonVisibility), typeof(Visibility), typeof(InformationAgreement));
    
    public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register(nameof(CancelButtonText), typeof(string), typeof(InformationAgreement));
        
    public static readonly DependencyProperty ConfirmButtonTextProperty = DependencyProperty.Register(nameof(ConfirmButtonText), typeof(string), typeof(InformationAgreement));
    
    public string InformationTitle
    {
        get => (string)GetValue(InformationTitleProperty);
        set => SetValue(InformationTitleProperty, value);
    }
        
    public string InformationMessage
    {
        get => (string)GetValue(InformationMessageProperty);
        set => SetValue(InformationMessageProperty, value);
    }

    public Visibility CancelButtonVisibility
    {
        get => (Visibility)GetValue(CancelButtonVisibilityProperty);
        set => SetValue(CancelButtonVisibilityProperty, value);
    }
    
    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }
    
    public string ConfirmButtonText
    {
        get => (string)GetValue(ConfirmButtonTextProperty);
        set => SetValue(ConfirmButtonTextProperty, value);
    }
        
    public DelegateCommand ConfirmCommand { get; set; }
    public DelegateCommand CancelCommand { get; set; }
    
    public InformationAgreement()
    {
        InitializeComponent();
        DataContext = this;
    }
}