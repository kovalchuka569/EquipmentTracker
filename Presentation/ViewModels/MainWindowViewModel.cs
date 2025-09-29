using System.Reflection;
using JetBrains.Annotations;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace Presentation.ViewModels;

public class MainWindowViewModel : BindableBase
{
    #region Dependencies
    
    private readonly IRegionManager _regionManager;
    
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
    
    public MainWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
        
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
        _regionManager.RequestNavigate("MainRegion", "AuthorizationView");
    }
    
    #endregion
}