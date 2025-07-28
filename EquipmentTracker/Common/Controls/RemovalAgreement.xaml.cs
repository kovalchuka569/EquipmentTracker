using System.Windows;
using Syncfusion.Windows.Shared;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace EquipmentTracker.Common.Controls;

public partial class RemovalAgreement : ChromelessWindow
{
    public static readonly DependencyProperty DeleteTitleProperty = DependencyProperty.Register("DeleteTitle", typeof(string), typeof(RemovalAgreement));
        
    public static readonly DependencyProperty DeleteMessageProperty = DependencyProperty.Register("DeleteMessage", typeof(string), typeof(RemovalAgreement));
        
    public string DeleteTitle
    {
        get => (string)GetValue(DeleteTitleProperty);
        set => SetValue(DeleteTitleProperty, value);
    }
        
    public string DeleteMessage
    {
        get => (string)GetValue(DeleteMessageProperty);
        set => SetValue(DeleteMessageProperty, value);
    }
        
    public DelegateCommand DeleteCommand { get; set; }
    public DelegateCommand CancelCommand { get; set; }

    public RemovalAgreement()
    {
        InitializeComponent();
        DataContext = this;
    }
}