
using Prism.Ioc;
using Prism.Modularity;
using Syncfusion.UI.Xaml.Diagram;
using Syncfusion.Windows.Controls.Navigation;
using Syncfusion.Windows.Tools.Controls;
using UI.ViewModels.Tabs;
using UI.ViewModels.TabControl;
using UI.Views.Tabs.Accounting;
using UI.Views.Tabs.Consumables;
using UI.Views.Tabs.EquipmentTree;
using UI.Views.Tabs.Scheduler;
using UI.Views.Tabs.Settings;
using Prism.Navigation.Regions;
using UI.Views.Tabs.EquipmentTree;
using System.ComponentModel;
using FurnitureTreeView = UI.Views.Tabs.EquipmentTree.FurnitureTreeView;
using OfficeTechniqueTreeView = UI.Views.Tabs.EquipmentTree.OfficeTechniqueTreeView;
using ToolsTreeView = UI.Views.Tabs.EquipmentTree.ToolsTreeView;

namespace UI.Views.TabControl;

public class TabControlModule : IModule
{
    private readonly RegionAdapterMappings _regionAdapterMappings;
    private readonly IContainerProvider _containerProvider;

    public TabControlModule(RegionAdapterMappings regionAdapterMappings, IContainerProvider containerProvider)
    {
        _regionAdapterMappings = regionAdapterMappings;
        _containerProvider = containerProvider;
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        _regionAdapterMappings.RegisterMapping(typeof(TabControlExt), _containerProvider.Resolve<TabControlExtRegionAdapter>());
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<TabControlView>();
        
        containerRegistry.RegisterForNavigation<EquipmentTreeView, EquipmentTreeViewModel>();
        containerRegistry.RegisterForNavigation<FurnitureTreeView, EquipmentTreeViewModel>();
        containerRegistry.RegisterForNavigation<OfficeTechniqueTreeView, EquipmentTreeViewModel>();
        containerRegistry.RegisterForNavigation<ToolsTreeView, EquipmentTreeViewModel>();

        containerRegistry.RegisterForNavigation<SchedulerView>();
        containerRegistry.RegisterForNavigation<SettingsView>();
        containerRegistry.RegisterForNavigation<ConsumablesView>();
        containerRegistry.RegisterForNavigation<AccountingView>();
        containerRegistry.RegisterForNavigation<FurnitureTreeView>();
        containerRegistry.RegisterForNavigation<OfficeTechniqueTreeView>();
        containerRegistry.RegisterForNavigation<ToolsTreeView>();

    }
}