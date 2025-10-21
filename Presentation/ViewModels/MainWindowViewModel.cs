using System.Reflection;
using JetBrains.Annotations;
using Presentation.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using Unity;

namespace Presentation.ViewModels;

public class MainWindowViewModel : BindableBase
{
    #region Dependencies
    
    [Dependency]
    public required IRegionManager RegionManager = null!;
    
    #endregion
    
    #region Private fields

    private string _currentVersion = $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";
    
    #endregion

    #region Properties
    
    public string CurrentVersion
    {
        get => _currentVersion;
        set => SetProperty(ref _currentVersion, value);
    }
    
    #endregion
    
    #region Constructor
    
    public MainWindowViewModel()
    {
        InitializeCommands();
    }
    
    #endregion

    #region Commands management
    
    public DelegateCommand MainWindowLoadedCommand { [UsedImplicitly] get; set; } = null!;

    private void InitializeCommands()
    {
        MainWindowLoadedCommand = new DelegateCommand(OnMainWindowLoadedCommandExecute);
    }
    
    #endregion

    #region Private methods
    
    private void OnMainWindowLoadedCommandExecute()
    {
        RegionManager.RequestNavigate("MainRegion", nameof(AuthorizationView));
        RegionManager.RequestNavigate("SnackbarRegion", nameof(SnackbarView));
    }
    
    #endregion
}