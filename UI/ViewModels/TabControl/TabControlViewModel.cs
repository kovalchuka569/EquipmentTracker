using System.Collections.ObjectModel;
using Core.Models.TabControl;
using Core.Services.TreeView;
using Microsoft.EntityFrameworkCore.Query;
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

namespace UI.ViewModels.TabControl;

public class TabControlViewModel : NotificationObject, INavigationAware
{
    private ObservableCollection<TabControlModel> _tabItems;
    private TabControlModel _selectedTabItem;
    private readonly IEventAggregator _eventAggregator;
    private readonly IContainerProvider _containerProvider;

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
    
    public TabControlViewModel(IEventAggregator eventAggregator, IContainerProvider containerProvider)
    {
        _eventAggregator = eventAggregator;
        _containerProvider = containerProvider;
        TabItems = new ObservableCollection<TabControlModel>();

        _eventAggregator.GetEvent<Core.Events.TabControl.OpenTabEvent>().Subscribe(OpenNewTab);
    }

    private void OpenNewTab(string tabHeader)
    {
        var existingTab = TabItems.FirstOrDefault(t => t.Header == tabHeader);
        if (existingTab != null)
        {
            SelectedTabItem = existingTab;
        }
        else
        {
           var newTabItem = new TabControlModel
           {
               Header = tabHeader,
               Content = new ProductionEquipmentGridView()
           }
           ;
           TabItems.Add(newTabItem);
           SelectedTabItem = newTabItem;
        }
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
       var selectedTab = navigationContext.Parameters["SelectedTab"] as string;
       
       
       var existingTab = TabItems.FirstOrDefault(t => t.Header == selectedTab);

       if (existingTab != null)
       {
           SelectedTabItem = existingTab;
       }
       else
       {
           TabControlModel? newTab = selectedTab switch
           {
                "Виробниче обладнання" => new TabControlModel{ Header = "Виробниче обладнання", Content = _containerProvider.Resolve<EquipmentTreeView>()},
                "Інструменти" => new TabControlModel{Header = "Інструменти", Content = _containerProvider.Resolve<ToolsTreeView>()},
                "Меблі" => new TabControlModel{Header = "Меблі", Content = _containerProvider.Resolve<FurnitureTreeView>()},
                "Офісна техніка" => new TabControlModel{Header = "Офісна техніка", Content = _containerProvider.Resolve<OfficeTechniqueTreeView>()},
                "Розхідні матеріали" => new TabControlModel{ Header = "Розхідні матеріали", Content = _containerProvider.Resolve<ConsumablesView>()},
                "Облік" => new TabControlModel{ Header = "Облік", Content = _containerProvider.Resolve<AccountingView>()},
                "Календар" => new TabControlModel{ Header = "Календар", Content = _containerProvider.Resolve<SchedulerView>()},
                "Налаштування" => new TabControlModel{ Header = "Налаштування", Content = _containerProvider.Resolve<SettingsView>()},
                _ => null
           };
           if (newTab != null)
           {
               TabItems.Add(newTab);
               SelectedTabItem = newTab;
               
           }
       }
    }
    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }
    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}