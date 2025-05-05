using Core.Models.DataGrid;
using Core.Services.Common.DataGridColumns;
using Core.Services.Consumables;
using Core.Services.Consumables.Operations;
using Core.Services.DataGrid;
using UI.ViewModels.Tabs;
using UI.ViewModels.TabControl;
using UI.Views.NavDrawer.NavDrawerItems.EquipmentTree;
using UI.ViewModels.DataGrid;
using UI.Views.NavDrawer.NavDrawerItems.EquipmentTree.ColumnSelector;

using Core.Services.TabControlExt;
using Data.Repositories.Common.DataGridColumns;
using Data.Repositories.Consumables;
using Data.Repositories.Consumables.Operations;
using Data.Repositories.DataGrid;
using UI.Interfaces.Factory;
using UI.Services.Factory;
using UI.ViewModels.Consumables;
using UI.ViewModels.Consumables.DetailsConsumables;
using UI.ViewModels.ConsumablesTree;
using UI.ViewModels.EquipmentTree;
using UI.Views;
using UI.Views.Consumables;
using UI.Views.Consumables.DetailsConsumables;
using UI.Views.DataGrid;
using UI.Views.NavDrawer.NavDrawerItems;
using UI.Views.NavDrawer.NavDrawerItems.ConsumablesTree;
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
        
        //Consumables tree
        containerRegistry.RegisterForNavigation<ConsumablesTreeView, ConsumablesTreeViewModel>(); // View, ViewModel
        containerRegistry.Register<IConsumablesTreeService, ConsumablesTreeService>(); // Service
        
        // Consumables DataGrid
        containerRegistry.RegisterForNavigation<ConsumablesDataGridView, ConsumablesDataGridViewModel>(); // View, ViewModel
        containerRegistry.Register<IConsumablesDataGridService, ConsumablesDataGridService>(); // Service
        containerRegistry.Register<IConsumablesDataGridRepository, ConsumablesDataGridRepository>(); // Repository
        
        //Details consumables
        containerRegistry.RegisterForNavigation<DetailsConsumablesView, DetailsConsumablesViewModel>(); // View, ViewModel
        //Operations DataGrid consumables
        containerRegistry.RegisterForNavigation<OperationsDataGridView, OperationsDataGridViewModel>(); // View, ViewModel
        containerRegistry.Register<IOperationsDataGridService, OperationsDataGridService>(); // Service
        containerRegistry.Register<IOperationsDataGridRepository, OperationsDataGridRepository>(); // Repository
        //Add new operation template
        containerRegistry.RegisterForNavigation<AddNewOperationView, AddNewOperationViewModel>(); // View, ViewModel
        
        //Common DaraGrid
        containerRegistry.Register<IDataGridColumnsService, DataGridColumnsService>(); // Columns service
        containerRegistry.Register<IDataGridColumnRepository, DataGridColumnRepository>(); // Columns repository
        
        // Other tabs 
        containerRegistry.RegisterForNavigation<SchedulerView, SchedulerViewModel>();
        containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
        containerRegistry.RegisterForNavigation<AccountingView, AccountingViewModel>();

        #endregion
    }
}