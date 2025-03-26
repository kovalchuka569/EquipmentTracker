using System.Collections.ObjectModel;
using System.Windows.Forms;
using Core.Models.TabControl;
using Core.Services.TreeView; // Добавь это для IRegionManager
using Syncfusion.Lic.util.encoders;
using Syncfusion.PMML;
using Syncfusion.Windows.Shared;
using UI.Views.Grids;
using UI.Views.Tabs.Accounting;
using UI.Views.Tabs.Consumables;
using UI.Views.Tabs.Furniture;
using UI.Views.Tabs.OfficeTechnique;
using UI.Views.Tabs.ProductionEquipmentTree;
using UI.Views.Tabs.Scheduler;
using UI.Views.Tabs.Settings;
using UI.Views.Tabs.ToolsTree;
using Header = Syncfusion.UI.Xaml.Diagram.Stencil.Header;
using UserControl = System.Windows.Controls.UserControl;

namespace UI.ViewModels.TabControl;

public class TabControlViewModel : NotificationObject, INavigationAware
{
    private ObservableCollection<TabControlModel> _tabItems;
    private TabControlModel _selectedTabItem;
    private readonly IEventAggregator _eventAggregator;
    private readonly IRegionManager _regionManager; 

    public ObservableCollection<TabControlModel> TabItems
    {
        get => _tabItems;
        set
        {
            _tabItems = value;
            this.RaisePropertyChanged(nameof(TabItems));
        }
    }

    public TabControlModel SelectedTabItem
    {
        get => _selectedTabItem;
        set
        {
            _selectedTabItem = value;
            this.RaisePropertyChanged(nameof(SelectedTabItem));
        }
    }

    public TabControlViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
    {
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        TabItems = new ObservableCollection<TabControlModel>();

        _eventAggregator.GetEvent<Core.Events.TabControl.OpenTabEvent>().Subscribe(OpenNewTab);
    }

    private void OpenNewTab(string tabHeader)
    {
        NavigateToTab(tabHeader, GetViewName(tabHeader));
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        var selectedTab = navigationContext.Parameters["SelectedTab"] as string;
        if (!string.IsNullOrEmpty(selectedTab))
        {
            NavigateToTab(selectedTab, GetViewName(selectedTab));
        }
    }

    private void NavigateToTab(string header, string viewName)
    {
        var parameters = new NavigationParameters
        {
            { "MenuType", header }
        };
        _regionManager.RequestNavigate("TabControlRegion", viewName, parameters);
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