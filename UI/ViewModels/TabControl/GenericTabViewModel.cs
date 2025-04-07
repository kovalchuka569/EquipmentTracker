using System.Windows;

namespace UI.ViewModels.TabControl;

public class GenericTabViewModel: BindableBase, INavigationAware
{
    private readonly IRegionManager _regionManager;
    private string _tabParameter;
    private string _uniqueRegionName;
    
    // Static dictionary for tracking active tabs and their regions
    private static readonly Dictionary<string, TabInfo> TabToRegionMap = new Dictionary<string, TabInfo>();
    
    public string TabParameter
    {
        get => _tabParameter;
        set => SetProperty(ref _tabParameter, value);
    }
    
    // The name of the region that will be used in XAML via binding
    public string UniqueRegionName
    {
        get => _uniqueRegionName;
        private set => SetProperty(ref _uniqueRegionName, value);
    }

    public GenericTabViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        // Get the parameter that will be used as the title and tab ID
        TabParameter = navigationContext.Parameters.GetValue<string>("parameter");
        Console.WriteLine($"GenericTabViewModel.OnNavigatedTo: Parameter = {TabParameter}");
        
        //Always create a new unique region name when navigating
        UniqueRegionName = $"TabContentRegion_{TabParameter}_{Guid.NewGuid().ToString("N")}";
        
        // Saving information about the tab
        TabToRegionMap[TabParameter] = new TabInfo 
        { 
            RegionName = UniqueRegionName,
            ViewModel = this
        };
        
        Console.WriteLine($"Created new region: {UniqueRegionName} for tab {TabParameter}");
        
        // Load the required view depending on the parameter
        string? viewToLoad = DetermineViewToLoad(TabParameter);
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
                            { "parameter", TabParameter }
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
    
    private string? DetermineViewToLoad(string parameter)
    {
        return parameter switch
        {
            "Виробниче обладнання" => "EquipmentTreeView",
            "Інструменти" => "EquipmentTreeView",
            "Меблі" => "EquipmentTreeView",
            "Офісна техніка" => "EquipmentTreeView",
            "Розхідні матеріали" => "ConsumablesView",
            "Облік" => "AccountingView",
            "Календар" => "SchedulerView",
            "Налаштування" => "SettingsView",
            _ => null // If the parameter does not match any of the known ones, return null
        };
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return false;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        // When navigating away from this view (closing a tab)
        Console.WriteLine($"Navigated from tab {TabParameter} with region {UniqueRegionName}");
        
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
}