
using Core.Services.Common;
using Core.Services.Consumables;
using Core.Services.Consumables.Operations;
using Core.Services.RepairsDataGrid;
using Core.Services.ServicesDataGrid;
using UI.ViewModels.Tabs;
using UI.ViewModels.DataGrid;
using Data.Repositories.Consumables;
using Data.Repositories.Consumables.Operations;
using Data.Repositories.Repairs;
using Data.Repositories.Services;
using EquipmentTracker.ViewModels.DataGrid.Services;
using EquipmentTracker.Views.DataGrid.Services;
using UI.ViewModels.Common;
using UI.ViewModels.Consumables;
using UI.ViewModels.Consumables.DetailsConsumables;
using UI.ViewModels.ConsumablesTree;
using UI.Views.Common;
using UI.Views.Consumables;
using UI.Views.Consumables.DetailsConsumables;
using UI.Views.DataGrid.Repairs;
using UI.Views.NavDrawer.NavDrawerItems;
using UI.Views.NavDrawer.NavDrawerItems.ConsumablesTree;
using UI.ViewModels.DataGrid.Services;
using UI.Views.DataGrid.Services;

namespace UI.Modules;

public class TabControlModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider) { }
    
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        
        #region Tabs
        
        // Image Viewer
        containerRegistry.RegisterForNavigation<ImageViewerView, ImageViewerViewModel>();
        containerRegistry.RegisterSingleton<ImageViewerTempStorage>();
        
        
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
        
        // Other tabs 
        containerRegistry.RegisterForNavigation<SchedulerView, SchedulerViewModel>();
        containerRegistry.RegisterForNavigation<AccountingView, AccountingViewModel>();

        #endregion
    }
}