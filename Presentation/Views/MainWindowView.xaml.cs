using System.Collections.ObjectModel;

using Syncfusion.Windows.Shared;

namespace Presentation.Views;

public partial class MainWindowView : ChromelessWindow
{
    public MainWindowView()
    {
        InitializeComponent();
    }
}

public class VersionTemplate : ObservableCollection<object> { }