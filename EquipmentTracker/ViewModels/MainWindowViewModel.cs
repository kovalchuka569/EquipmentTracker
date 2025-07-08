using System.Reflection;

namespace UI.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private string _currentVersion = $"v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

    public string CurrentVersion
    {
        get => _currentVersion;
        set => SetProperty(ref _currentVersion, value);
    }
}