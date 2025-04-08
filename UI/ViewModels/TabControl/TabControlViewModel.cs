using System.Windows;
using Core.Events.EquipmentTree;
using Core.Events.NavDrawer;
using Syncfusion.Windows.Shared;
using Prism.Commands;
using UI.ViewModels.DataGrid;
using UI.ViewModels.Tabs;
using Syncfusion.Windows.Tools.Controls;
using UI.ViewModels.NavDrawer;
using DelegateCommand = Prism.Commands.DelegateCommand;


namespace UI.ViewModels.TabControl;

public class TabControlViewModel : BindableBase, INavigationAware
{
    
    private readonly IRegionManager _regionManager;
    private readonly IEventAggregator _eventAggregator;
    private string _tabView;
        
    public Prism.Commands.DelegateCommand<object> CloseSelectedTabCommand { get; private set; }
    public DelegateCommand CloseAllTabsCommand { get; private set; }
    public Prism.Commands.DelegateCommand<object> CloseOtherTabsCommand { get; private set; }
   
    public TabControlViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        
        CloseSelectedTabCommand = new Prism.Commands.DelegateCommand<object>(CloseSelectedTab);
        CloseAllTabsCommand = new DelegateCommand(CloseAllTabs);
        CloseOtherTabsCommand = new Prism.Commands.DelegateCommand<object>(CloseOtherTabs);
        
        _eventAggregator.GetEvent<OnOpenFileEvent>().Subscribe(NavigateFromFile);
        _eventAggregator.GetEvent<OpenEquipmentTreeTabEvent>().Subscribe(NavigateFromNavDrawer);
        _eventAggregator.GetEvent<OpenOtherTabEvent>().Subscribe(NavigateFromOther);
    }

    private void NavigateFromOther(string tabName)
    {
        
        Console.WriteLine($"Navigating to {tabName}");
        if (string.IsNullOrEmpty(tabName)) 
            return;

        Console.WriteLine($"Navigate called with parameter: {tabName}");

        // Get the region
        var region = _regionManager.Regions["TabControlRegion"];
        string tabType = "Other";
    
        // Create navigation parameters with tabName as a parameter
        var parameters = new NavigationParameters
        {
            { "TabName", tabName },
            { "TabType", tabType }
        };

        // Check if such a tab already exists
        var existingView = region.Views
            .OfType<FrameworkElement>()
            .FirstOrDefault(v => {
                if (v.DataContext is GenericTabViewModel vm)
                {
                    return vm.TabName == tabName;
                }
                return false;
            });

        if (existingView != null)
        {
            // Activate an existing view
            region.Activate(existingView);
        
            // If the view is inside a TabItemExt, switch the tab
            if (existingView.Parent is TabItemExt tabItem && tabItem.Parent is TabControlExt tabControl)
            {
                tabControl.SelectedItem = tabItem;
            }
            return;
        }
    
        // If the view is not found, navigate to the GenericTabView with parameters
        _regionManager.RequestNavigate("TabControlRegion", "GenericTabView", parameters);
    }
    
    private void NavigateFromFile(string tabName)
    {
        Console.WriteLine($"Navigating to {tabName}");
        if (string.IsNullOrEmpty(tabName)) 
            return;
        Console.WriteLine($"Navigate called with parameter: {tabName}");
        // Get the region
        var region = _regionManager.Regions["TabControlRegion"];
        string fileName = tabName;
        string tabType = "File";
        var parameters = new NavigationParameters
        {
            { "TabName", tabName},
            { "FileName", fileName },
            { "TabType", tabType }
        };
        // Check if such a tab already exists
        var existingView = region.Views
            .OfType<FrameworkElement>()
            .FirstOrDefault(v => {
                if (v.DataContext is GenericTabViewModel vm)
                {
                    return vm.TabName == tabName;
                }
                return false;
            });

        if (existingView != null)
        {
            // Activate an existing view
            region.Activate(existingView);
        
            // If the view is inside a TabItemExt, switch the tab
            if (existingView.Parent is TabItemExt tabItem && tabItem.Parent is TabControlExt tabControl)
            {
                tabControl.SelectedItem = tabItem;
            }
            return;
        }

        // If the view is not found, navigate to the GenericTabView with parameters
        _regionManager.RequestNavigate("TabControlRegion", "GenericTabView", parameters);
    }
    
     private void NavigateFromNavDrawer(string tabName)
        {
            Console.WriteLine($"Navigating to {tabName}");
            if (string.IsNullOrEmpty(tabName)) 
                return;

            Console.WriteLine($"Navigate called with parameter: {tabName}");

            // Get the region
            var region = _regionManager.Regions["TabControlRegion"];
            string tabType = "EquipmentTree";
    
            // Create navigation parameters with tabName as a parameter
            var parameters = new NavigationParameters
            {
                { "TabName", tabName },
                { "TabType", tabType }
            };

            // Check if such a tab already exists
            var existingView = region.Views
                .OfType<FrameworkElement>()
                .FirstOrDefault(v => {
                    if (v.DataContext is GenericTabViewModel vm)
                    {
                        return vm.TabName == tabName;
                    }
                    return false;
                });

            if (existingView != null)
            {
                // Activate an existing view
                region.Activate(existingView);
        
                // If the view is inside a TabItemExt, switch the tab
                if (existingView.Parent is TabItemExt tabItem && tabItem.Parent is TabControlExt tabControl)
                {
                    tabControl.SelectedItem = tabItem;
                }
                return;
            }

            // If the view is not found, navigate to the GenericTabView with parameters
            _regionManager.RequestNavigate("TabControlRegion", "GenericTabView", parameters);
        }

        private void CloseSelectedTab(object parameter)
        {
            Console.WriteLine("CloseSelectedTab called");
            Console.WriteLine($"Parameter type: {parameter?.GetType().Name}");
            
            var region = _regionManager.Regions["TabControlRegion"];
            Console.WriteLine($"Region found: {region != null}");

            if (parameter is TabItemExt tabItem)
            {
                Console.WriteLine($"TabItemExt content type: {tabItem.Content?.GetType().Name}");
                if (tabItem.Content is FrameworkElement content)
                {
                    Console.WriteLine("Removing content from region");
                    region.Remove(content);
                }
            }
        }

        private void CloseAllTabs()
        {
            Console.WriteLine("CloseAllTabs called");
            var region = _regionManager.Regions["TabControlRegion"];
            Console.WriteLine($"Region found: {region != null}");
            
            var views = region.Views.ToList();
            Console.WriteLine($"Views count: {views.Count}");
            foreach (var view in views)
            {
                Console.WriteLine("Removing view from region");
                region.Remove(view);
            }
        }

        private void CloseOtherTabs(object parameter)
        {
            Console.WriteLine("CloseOtherTabs called");
            Console.WriteLine($"Parameter type: {parameter?.GetType().Name}");
            
            var region = _regionManager.Regions["TabControlRegion"];
            Console.WriteLine($"Region found: {region != null}");

            if (parameter is TabItemExt tabItem && tabItem.Content is FrameworkElement selectedContent)
            {
                Console.WriteLine($"TabItemExt content type: {tabItem.Content.GetType().Name}");
                
                var viewsToRemove = region.Views.Cast<FrameworkElement>()
                    .Where(v => v != selectedContent)
                    .ToList();
                Console.WriteLine($"Views to remove count: {viewsToRemove.Count}");

                foreach (var viewToRemove in viewsToRemove)
                {
                    Console.WriteLine("Removing view from region");
                    region.Remove(viewToRemove);
                }
            }
        }



    public void OnNavigatedTo(NavigationContext navigationContext)
    {
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}