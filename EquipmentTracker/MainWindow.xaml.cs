
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using Syncfusion.SfSkinManager;
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