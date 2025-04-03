using System.Collections.ObjectModel;
using System.Windows.Forms;
using Core.Events.TabControl;
using Core.Models.TabControl;
using Core.Models.Tabs.ProductionEquipmentTree;
using Syncfusion.Lic.util.encoders;
using Syncfusion.PMML;
using Syncfusion.Windows.Shared;
using Prism.Commands;
using UI.ViewModels.DataGrid;
using UI.Views.Tabs.Accounting;
using UI.Views.Tabs.Consumables;
using UI.Views.Tabs.EquipmentTree;
using UI.Views.Tabs.Scheduler;
using UI.Views.Tabs.Settings;
using DataGridView = UI.Views.DataGridView;
using DelegateCommand = Syncfusion.Windows.Shared.DelegateCommand;
using Header = Syncfusion.UI.Xaml.Diagram.Stencil.Header;
using UserControl = System.Windows.Controls.UserControl;

namespace UI.ViewModels.TabControl;

public class TabControlViewModel : BindableBase, INavigationAware
{
    private ObservableCollection<TabControlModel> _tabItems;
    private TabControlModel _selectedTabItem;
    private readonly IEventAggregator _eventAggregator;
    public TabControlModel SelectedTabItem
    {
        get => _selectedTabItem;
        set
        {
            _selectedTabItem = value;
            this.RaisePropertyChanged(nameof(SelectedTabItem));
        }
    }
    
    private readonly IRegionManager _regionManager;
    public ObservableCollection<TabControlModel> TabItems { get; set; }
    private TabControlModel _selectedTab;
    public TabControlModel SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (_selectedTab != value)
            {
                _selectedTab = value;
                RaisePropertyChanged();
                NavigateToSelectedTab();
            }
        }
    }
    
    public TabControlViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
    {
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        TabItems = new ObservableCollection<TabControlModel>();
        
    }
    

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        var menuType = navigationContext.Parameters["SelectedTab"] as string;

        var parameters = new NavigationParameters
        {
            { "MenuType", menuType },
        };
        string viewName = string.Empty;
        switch (menuType)
        {
            case "Виробниче обладнання":
                viewName = "EquipmentTreeView";
                break;
            case "Інструменти":
                viewName = "EquipmentTreeView";
                break;
            case "Меблі":
                viewName = "EquipmentTreeView";
                break;
            case "Офісна техніка":
                viewName = "EquipmentTreeView";
                break;
            case "Розхідні матеріали":
                viewName = "ConsumablesView";
                break;
            case "Облік":
                viewName = "AccountingView";
                break;
            case "Календар":
                viewName = "SchedulerView";
                break;
            case "Налаштування":
                viewName = "SettingsView";
                break;
        }
        Console.WriteLine("ViewName: " + viewName);
        Console.WriteLine("MenuType:" + menuType);
        
        CreatingTab(viewName, menuType);
    }
    
    public void OnTabSelectionChanged(TabControlModel selectedTab)
    {
        if (selectedTab == null)
        {
            _regionManager.Regions["TabContentRegion"].Context = null;
        }
    }

    private void NavigateToSelectedTab()
    {
        if (TabItems.Count == 0)
        {
            _regionManager.RequestNavigate("TabContentRegion", "DefaultTabContentView");
        }
        
        if (SelectedTab != null)
        {
            var parameters = new NavigationParameters
            {
                { "MenuType", SelectedTab.Header }
            };

            _regionManager.RequestNavigate("TabContentRegion", SelectedTab.ViewName, parameters);
        }
    }

    private void CreatingTab(string viewName, string menuType)
    {
        var existingTab = TabItems.FirstOrDefault(t => t.Header == menuType);
        
        var parameters = new NavigationParameters
        {
            { "MenuType", menuType }
        };

        if (existingTab != null)
        {
            SelectedTab = existingTab;
            _regionManager.RequestNavigate("TabContentRegion", existingTab.ViewName, parameters);
            return;
        }
        
        Console.WriteLine("Создание вкладки: " + viewName);
        
            TabControlModel newTab = new TabControlModel
            {
                Header = menuType,
                ViewName = viewName
            };

            _regionManager.RequestNavigate("TabContentRegion", viewName, result =>
            {
                if (result.Success)
                {
                    Console.WriteLine("Навигация успешна: " + viewName);
                    TabItems.Add(newTab);
                    SelectedTab = newTab;
                }
                else
                {
                    Console.WriteLine("Ошибка навигации: " + viewName);
                }
            }, parameters);
    }
    
    private string GetViewName(string header)
    {
        return header switch
        {
            "Виробниче обладнання" => nameof(EquipmentTreeView),
            "Інструменти" => nameof(ToolsTreeView),
            "Меблі" => nameof(FurnitureTreeView),
            "Офісна техніка" => nameof(OfficeTechniqueTreeView),
            "Розхідні матеріали" => nameof(ConsumablesView),
            "Облік" => nameof(AccountingView),
            "Календар" => nameof(SchedulerView),
            "Налаштування" => nameof(SettingsView),
            _ => throw new ArgumentException($"Unknown tab header: {header}")
        };
    }
    

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}