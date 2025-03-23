using Core.Services.TreeView;
using UI.ViewModels.Tabs;
using UI.Views.Tabs.Accounting;
using UI.Views.Tabs.Consumables;
using UI.Views.Tabs.Furniture;
using UI.Views.Tabs.OfficeTechnique;
using UI.Views.Tabs.ProductionEquipmentTree;
using UI.Views.Tabs.Scheduler;
using UI.Views.Tabs.Settings;
using UI.Views.Tabs.ToolsTree;

namespace UI.Views.TabControl;

public class TabControlModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<EquipmentTreeView, EquipmentTreeViewModel>();
        containerRegistry.RegisterSingleton<ITreeViewService, TreeViewService>();
        
        containerRegistry.RegisterForNavigation<SchedulerView>();
        containerRegistry.RegisterForNavigation<SettingsView>();
        containerRegistry.RegisterForNavigation<ConsumablesView>();
        containerRegistry.RegisterForNavigation<AccountingView>();
        
        containerRegistry.RegisterForNavigation<EquipmentTreeView>();
        containerRegistry.RegisterForNavigation<FurnitureTreeView>();
        containerRegistry.RegisterForNavigation<OfficeTechniqueTreeView>();
        containerRegistry.RegisterForNavigation<ToolsTreeView>();
    }

}