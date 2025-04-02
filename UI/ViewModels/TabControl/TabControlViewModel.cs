using System.Collections.ObjectModel;
using System.Windows.Forms;
using Core.Events.TabControl;
using Core.Models.TabControl;
using Core.Models.Tabs.ProductionEquipmentTree;
using Syncfusion.Lic.util.encoders;
using Syncfusion.PMML;
using Syncfusion.Windows.Shared;
using UI.ViewModels.DataGrid;
using UI.Views.Tabs.Accounting;
using UI.Views.Tabs.Consumables;
using UI.Views.Tabs.EquipmentTree;
using UI.Views.Tabs.Scheduler;
using UI.Views.Tabs.Settings;
using DataGridView = UI.Views.DataGridView;
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
            if (SetProperty(ref _selectedTab, value))
            {
                OnTabSelectionChanged(value);
            }
        }
    }

    public TabControlViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
    {
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        TabItems = new ObservableCollection<TabControlModel>();

        _eventAggregator.GetEvent<OpenTabEvent>().Subscribe(OpenNewTab);
    }
    

    private void OpenNewTab(string tabHeader)
    {
        NavigateToTab(tabHeader, GetViewName(tabHeader));
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
        
    }

    private void NavigateToTab(string header, string viewName)
    {
        var parameters = new NavigationParameters
        {
            { "SelectedTab", header }
        };
        Console.WriteLine("OnNavigatedTo: " + header);

    }

    private void CreatingTab(string viewName, string menuType)
    {
        var parameters = new NavigationParameters();
        parameters.Add("MenuType", menuType);
        Console.WriteLine("Создание вкладки: " + viewName);

        var existingTab = TabItems.FirstOrDefault(t => t.ViewName == viewName);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        TabControlModel newTab = new TabControlModel
        {
            Header = viewName,
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