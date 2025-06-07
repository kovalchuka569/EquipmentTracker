using Core.Models.DataGrid;
using Core.Services.Common;
using Core.Services.Common.DataGridColumns;
using Core.Services.Consumables;
using Core.Services.Consumables.Operations;
using Core.Services.DataGrid;
using Core.Services.EquipmentDataGrid;
using Core.Services.EquipmentTree;
using Core.Services.RepairsDataGrid;
using Core.Services.ServicesDataGrid;
using UI.ViewModels.Tabs;
using UI.ViewModels.TabControl;
using UI.Views.NavDrawer.NavDrawerItems.EquipmentTree;
using UI.ViewModels.DataGrid;
using UI.Views.NavDrawer.NavDrawerItems.EquipmentTree.ColumnSelector;
using Data.Repositories.Common.DataGridColumns;
using Data.Repositories.Consumables;
using Data.Repositories.Consumables.Operations;
using Data.Repositories.DataGrid;
using Data.Repositories.EquipmentDataGrid;
using Data.Repositories.EquipmentTree;
using Data.Repositories.Repairs;
using Data.Repositories.Services;
using EquipmentTracker.ViewModels.DataGrid;
using EquipmentTracker.ViewModels.DataGrid.Services;
using EquipmentTracker.Views.DataGrid;
using EquipmentTracker.Views.DataGrid.Services;
using UI.Interfaces.Factory;
using UI.Services.Factory;
using UI.ViewModels.Common;
using UI.ViewModels.Consumables;
using UI.ViewModels.Consumables.DetailsConsumables;
using UI.ViewModels.ConsumablesTree;
using UI.ViewModels.EquipmentTree;
using UI.ViewModels.TabControl.GenericTab;
using UI.Views;
using UI.Views.Common;
using UI.Views.Consumables;
using UI.Views.Consumables.DetailsConsumables;
using UI.Views.DataGrid;
using UI.Views.DataGrid.Repairs;
using UI.Views.NavDrawer.NavDrawerItems;
using UI.Views.NavDrawer.NavDrawerItems.ConsumablesTree;
using UI.Views.TabControl;
using Prism.Modularity;
using Prism.Ioc;
using Prism.Regions;
using UI.ViewModels.DataGrid.Services;
using UI.Views.DataGrid.Services;

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
        
        // Generic tab 
        containerRegistry.Register<IGenericTabViewModelFactory, GenericTabViewModelFactory>();
        containerRegistry.RegisterForNavigation<GenericTabView, GenericTabViewModel>();
        
        #region Tabs
        
        // Image Viewer
        containerRegistry.RegisterForNavigation<ImageViewerView, ImageViewerViewModel>();
        containerRegistry.RegisterSingleton<ImageViewerTempStorage>();
        
        // Equipment tree
        containerRegistry.RegisterForNavigation<EquipmentTreeView, EquipmentTreeViewModel>(); // View, ViewModel
        containerRegistry.Register<IEquipmentTreeService, EquipmentTreeService>(); // Service
        containerRegistry.Register<IEquipmentTreeRepository, EquipmentTreeRepository>(); // Repository
        
        // Column selector in Equipment tree
        containerRegistry.RegisterForNavigation<ColumnSelectorView, ColumnSelectorViewModel>();
        
        // Data grid
        containerRegistry.RegisterForNavigation<DataGridView, DataGridViewModel>(); // View, ViewModel
        
        containerRegistry.RegisterForNavigation<EquipmentDataGridView, EquipmentDataGridViewModel>(); // View, ViewModel
        containerRegistry.Register<IEquipmentDataGridService, EquipmentDataGridService>(); // Service
        containerRegistry.Register<IEquipmentDataGridRepository, EquipmentDataGridRepository>(); // Repository
        // Deletion agreement
        containerRegistry.RegisterForNavigation<DeletionAgreementView, DeletionAgreementViewModel>();
        
        containerRegistry.Register<IDataGridViewModelFactory, DataGridViewModelFactory>(); // Factory for ViewModel
        containerRegistry.Register<IDataGridService, DataGridService>(); // Service for DataGrid
        containerRegistry.Register<ISparePartsService, SparePartsService>(); // Service for SpareParts
        containerRegistry.Register<IDataGridColumnService, DataGridColumnService>(); // Service for columns SfDataGrid
        containerRegistry.Register<IDataGridRepository, DataGridRepository>(); // Repository for DataGrid
        containerRegistry.Register<ISparePartsRepository, SparePartsRepository>(); // Repository for SpareParts
        
        //Write off data grid
        containerRegistry.RegisterForNavigation<WriteOffDataGridView, WriteOffDataGridViewModel>(); // View, ViewModel
        
        //Services data grid
        containerRegistry.RegisterForNavigation<ServicesDataGridView, ServicesDataGridViewModel>(); // View, ViewModel
        containerRegistry.Register<IServicesDataGridService, ServicesDataGridService>(); // Service
        containerRegistry.Register<IServicesDataGridReposotory, ServicesDataGridRepository>(); // Repository
                // Add service template
                containerRegistry.RegisterForNavigation<AddServiceView, AddServiceViewModel>(); // View, ViewModel
                containerRegistry.Register<IAddServiceService, AddServiceService>(); // Service
                containerRegistry.Register<IAddServiceRepository, AddServiceRepository>(); // Repository
                
        
        //Repairs data grid
        containerRegistry.RegisterForNavigation<RepairsDataGridView, RepairsDataGridViewModel>(); // View, ViewModel
        containerRegistry.Register<IRepairsDataGridService, RepairsDataGridService>(); // Service
        containerRegistry.Register<IRepairsRepository, RepairsRepository>(); // Repository
                //Add repair template
                containerRegistry.RegisterForNavigation<AddRepairView, AddRepairViewModel>(); // View, ViewModel
                containerRegistry.Register<IAddRepairService, AddRepairService>(); // Service
                containerRegistry.Register<IAddRepairRepository, AddRepairRepository>(); // Repository
                        //Data grid used materials
                        containerRegistry.RegisterForNavigation<DataGridUsedMaterialsView, DataGridUsedMaterialsViewModel>(); // View, VieModel
                        
        
        //Consumables tree
        containerRegistry.RegisterForNavigation<ConsumablesTreeView, ConsumablesTreeViewModel>(); // View, ViewModel
        containerRegistry.RegisterForNavigation<ConsumablesTreeSelectorView, ConsumablesTreeSelectorViewModel>(); // Consumables selector View, ViewModel
        containerRegistry.Register<IConsumablesTreeService, ConsumablesTreeService>(); // Service
        
        // Consumables DataGrid
        containerRegistry.RegisterForNavigation<ConsumablesDataGridView, ConsumablesDataGridViewModel>(); // View, ViewModel
        containerRegistry.RegisterForNavigation<ConsumablesDataGridSelectorView, ConsumablesDataGridSelectorViewModel>(); // Selector View, ViewModel
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