
using System.Collections.ObjectModel;
using System.Windows;
using Syncfusion.Windows.Shared;

namespace UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ChromelessWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
}

public class VersionTemplate : ObservableCollection<object> { }