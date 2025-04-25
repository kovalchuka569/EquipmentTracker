using Core.Models.DataGrid;
using Core.Services.DataGrid;
using UI.ViewModels.Tabs;
using UI.ViewModels.TabControl;
using UI.Views.NavDrawer.NavDrawerItems.EquipmentTree;
using UI.ViewModels.DataGrid;
using UI.Views.NavDrawer.NavDrawerItems.EquipmentTree.ColumnSelector;

using Core.Services.TabControlExt;
using Data.Repositories.DataGrid;
using UI.Interfaces.Factory;
using UI.Services.Factory;
using UI.ViewModels.EquipmentTree;
using UI.Views;
using UI.Views.DataGrid;
using UI.Views.NavDrawer.NavDrawerItems;
using UI.Views.TabControl;

namespace UI.Modules;

public class TabControlModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("EquipmentTreeColumnSelectorRegion", typeof(EquipmentTreeView));
    }
    
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Tab control (user control)
        containerRegistry.RegisterForNavigation<TabControlView, TabControlViewModel>();
        
        // Region manager service
        containerRegistry.RegisterSingleton<IRegionManagerService, RegionManagerService>();
        
        // Generic tab 
        containerRegistry.RegisterForNavigation<GenericTabView, GenericTabViewModel>();
        
        #region Tabs
        
        // Equipment tree
        containerRegistry.RegisterSingleton<IEquipmentTreeViewModelFactory, IEquipmentTreeViewModelFactory>(); // Factory
        containerRegistry.RegisterForNavigation<EquipmentTreeView, EquipmentTreeViewModel>();
        
        // Column selector in Equipment tree
        containerRegistry.RegisterForNavigation<ColumnSelectorView, ColumnSelectorViewModel>();
        
        // Data grid
        containerRegistry.RegisterForNavigation<DataGridView, DataGridViewModel>(); // View, ViewModel
        containerRegistry.Register<IDataGridViewModelFactory, DataGridViewModelFactory>(); // Factory for ViewModel
        containerRegistry.Register<IDataGridService, DataGridService>(); // Service for DataGrid
        containerRegistry.Register<ISparePartsService, SparePartsService>(); // Service for SpareParts
        containerRegistry.Register<IDataGridColumnService, DataGridColumnService>(); // Service for columns SfDataGrid
        containerRegistry.Register<IDataGridRepository, DataGridRepository>(); // Repository for DataGrid
        containerRegistry.Register<ISparePartsRepository, SparePartsRepository>(); // Repository for SpareParts
        
        // Other tabs 
        containerRegistry.RegisterForNavigation<SchedulerView, SchedulerViewModel>();
        containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
        containerRegistry.RegisterForNavigation<ConsumablesView, ConsumablesViewModel>();
        containerRegistry.RegisterForNavigation<AccountingView, AccountingViewModel>();

        #endregion
    }
}