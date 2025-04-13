using System.Windows;
using Core.Events;

namespace UI.ViewModels.TabControl;

public class GenericTabViewModel: BindableBase, INavigationAware, IDisposable
{
    private readonly IEventAggregator _eventAggregator;
    private readonly SubscriptionToken _busyIndicatorToken;
    private readonly IRegionManager _regionManager;
    private string _tabName;
    private string _tabType;
    private string _uniqueRegionName;
    private bool _isBusy;
    
    
    // Static dictionary for tracking active tabs and their regions
    private static readonly Dictionary<string, TabInfo> TabToRegionMap = new Dictionary<string, TabInfo>();
    
    public string TabName
    {
        get => _tabName;
        set => SetProperty(ref _tabName, value);
    }

    public string TabType
    {
        get => _tabType;
        set => SetProperty(ref _tabType, value);
    }
    
    // The name of the region that will be used in XAML via binding
    public string UniqueRegionName
    {
        get => _uniqueRegionName;
        private set => SetProperty(ref _uniqueRegionName, value);
    }

    // Grid visibility of busy indicator
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    
    public GenericTabViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
    {
        _regionManager = regionManager;
        _eventAggregator = eventAggregator;

        _busyIndicatorToken = _eventAggregator.GetEvent<BusyIndicatorEvent>().Subscribe(OnBusyIndicatorChanged);
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        // Get the parameter that will be used as the title and tab ID
        TabName = navigationContext.Parameters.GetValue<string>("TabName");
        TabType = navigationContext.Parameters.GetValue<string>("TabType");
        
        Console.WriteLine($"GenericTabViewModel.OnNavigatedTo: TabName = {TabName}");
        Console.WriteLine($"GenericTabViewModel.OnNavigatedTo: TabType = {TabType}");
        
        //Always create a new unique region name when navigating
        UniqueRegionName = $"TabContentRegion_{TabName}_{Guid.NewGuid().ToString("N")}";
        
        // Saving information about the tab
        TabToRegionMap[TabName] = new TabInfo 
        { 
            RegionName = UniqueRegionName,
            ViewModel = this
        };
        
        Console.WriteLine($"Created new region: {UniqueRegionName} for tab {TabName}");
        
        // Load the required view depending on the parameter
        string? viewToLoad = DetermineViewToLoad(TabType);
        if (!string.IsNullOrEmpty(viewToLoad))
        {
            // Wait until UniqueRegionName is bound to RegionManager.RegionName
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                try 
                {
                    // Give a little time to bind the region
                    System.Threading.Thread.Sleep(50);
                    
                    if (_regionManager.Regions.ContainsRegionWithName(UniqueRegionName))
                    {
                        // Loading the view
                        Console.WriteLine($"Loading view {viewToLoad} into region {UniqueRegionName}");
                        var parameters = new NavigationParameters
                        {
                            { "parameter", TabName }
                        };
                        _regionManager.RequestNavigate(UniqueRegionName, viewToLoad, parameters);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Region {UniqueRegionName} not found");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error navigating to {viewToLoad}: {ex.Message}");
                }
            }));
        }
    }
    
    private string? DetermineViewToLoad(string tabType)
    {
        if (tabType == "File")
        {
            // Для всех файлов используем DataGridView
            return "DataGridView";
        }
        if (tabType == "EquipmentTree")
        {
            // Для всех пунктов оборудования используем EquipmentTreeView
            return "EquipmentTreeView";
        }

        if (tabType == "Other")
        {
            return TabName switch
            {
                "Розхідні матеріали" => "ConsumablesView",
                "Облік" => "AccountingView",
                "Календар" => "SchedulerView",
                "Налаштування" => "SettingsView",
                _ =>"DefaultTabContentView"
            };
        }
        return null;
    }

    /// <summary>
    /// Receives from eventAgregattor the command when to show BusyIndicator
    /// </summary>
    private void OnBusyIndicatorChanged(bool isBusy)
    {
        IsBusy = isBusy;
    }
    
    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return false;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        // When navigating away from this view (closing a tab)
        Console.WriteLine($"Navigated from tab {TabName} with region {UniqueRegionName}");
        
        try
        {
            // Clear the region if it still exists
            if (_regionManager.Regions.ContainsRegionWithName(UniqueRegionName))
            {
                _regionManager.Regions.Remove(UniqueRegionName);
                Console.WriteLine($"Removed region {UniqueRegionName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing region {UniqueRegionName}: {ex.Message}");
        }
    }
    private class TabInfo
    {
        public string RegionName { get; set; }
        public GenericTabViewModel ViewModel { get; set; }
    }

    public void Dispose()
    {
        _eventAggregator.GetEvent<BusyIndicatorEvent>().Unsubscribe(_busyIndicatorToken);
    }
}