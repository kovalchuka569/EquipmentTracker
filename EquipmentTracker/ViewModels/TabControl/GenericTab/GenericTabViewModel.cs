
using Core.Common.RegionHelpers;

namespace UI.ViewModels.TabControl.GenericTab;

public class GenericTabViewModel : IDisposable
{
    private IScopedProvider _tabScopedServiceProvider;
    public IRegionManager ViewRegionManager { get; }
    private IEventAggregator _scopedEventAggregator;
    public Dictionary<string, object> Parameters { get; set; }
    public string _viewNameToShow;
    
    public DelegateCommand UserControlLoadedCommand { get; }
    public GenericTabViewModel(IRegionManager scopedRegionManager, IScopedProvider tabScopedServiceProvider)
    {
        ViewRegionManager = scopedRegionManager;
        _scopedEventAggregator = new EventAggregator();
        _tabScopedServiceProvider = tabScopedServiceProvider;
        
        UserControlLoadedCommand = new DelegateCommand(OnUserControlLoaded);
    }
    
    private bool _isContentRegionNavigated;
    private void OnUserControlLoaded()
    {   
        if (string.IsNullOrEmpty(_viewNameToShow)) return;

        if (!_isContentRegionNavigated)
        {
            var navigationParameters = new NavigationParameters();
            ExtractViewSpecificParameters(navigationParameters);
            navigationParameters.Add("ScopedRegionManager", ViewRegionManager);
            navigationParameters.Add("ScopedEventAggregator", _scopedEventAggregator);
            navigationParameters.Add("TabScopedServiceProvider", _tabScopedServiceProvider);
        
            ViewRegionManager.RequestNavigate("ContentRegion", _viewNameToShow, navigationParameters);

            _isContentRegionNavigated = true;
        }
    }
    
    private void ExtractViewSpecificParameters(NavigationParameters parameters)
    {
        if(parameters == null) return;
        
        var prefix = _viewNameToShow + ".";
        
        // Local parameters (for view with prefix)
        foreach (var param in Parameters)
        {
            if (param.Key.StartsWith(prefix))
            {
                var cleanKey = param.Key.Substring(prefix.Length);
                parameters.Add(cleanKey, param.Value);
            }
        }
        
        // Global parameters (for all views)
        foreach (var param in Parameters)
        {
            if(!param.Key.Contains(".") && !parameters.ContainsKey(param.Key))
            {
                parameters.Add(param.Key, param.Value);
            }
        }
    }
    
    public void InitializeParameters()
    {
        if (Parameters != null)
        {
            if (Parameters.ContainsKey("ViewNameToShow"))
            {
                _viewNameToShow = Parameters["ViewNameToShow"] as string; 
            }
        }
    }

    public void Dispose()
    {
        RegionCleanupHelper.CleanRegion(ViewRegionManager, "ContentRegion");
        if (_tabScopedServiceProvider != null)
        {
            _tabScopedServiceProvider.Dispose();
        }
    }
}