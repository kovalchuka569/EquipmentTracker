using Prism.Ioc;
using Prism.Modularity;

using Presentation.ViewModels;
using Presentation.Views;

using Presentation.Interfaces;
using Presentation.UIManagers;

namespace Presentation;

public class PresentationModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Interfaces and interfaces UIManagers
        containerRegistry.RegisterScoped<IGenericTabManager, GenericTabManager>();
        
        containerRegistry.RegisterScoped<ICleanRegionManager, CleanRegionManager>();

        containerRegistry.RegisterScoped<IExcelExportManager, ExcelExportManager>();

        containerRegistry.RegisterScoped<IPdfExportManager, PdfExportManager>();

        containerRegistry.RegisterScoped<ISyncfusionGridPrintManager, SyncfusionGridPrintManager>();
        
        containerRegistry.RegisterScoped<ISyncfusionGridColumnManager, SyncfusionGridColumnManager>();

        containerRegistry.RegisterScoped<IDialogManager, DialogManager>();
        
        containerRegistry.RegisterScoped<IGridInteractionManager, GridInteractionManager>();
        
        containerRegistry.RegisterScoped<IOverlayManager, OverlayManager>();
        
        // Views and view models
        containerRegistry.RegisterForNavigation<MainWindowView, MainWindowViewModel>();
        
        containerRegistry.RegisterForNavigation<GenericTabView, GenericTabViewModel>();
        
        containerRegistry.RegisterForNavigation<NavDrawerView, NavDrawerViewModel>();
        
        containerRegistry.RegisterForNavigation<MainTreeView, MainTreeViewModel>();
        
        containerRegistry.RegisterForNavigation<EquipmentSheetView, EquipmentSheetViewModel>();
        
        containerRegistry.RegisterForNavigation<DialogBoxView, DialogBoxViewModel>();
        
        containerRegistry.RegisterForNavigation<ExcelImportConfiguratorView, ExcelImportConfiguratorViewModel>();
        
        containerRegistry.RegisterForNavigation<PivotSheetView, PivotSheetViewModel>();
        
        containerRegistry.RegisterForNavigation<ColumnDesignerView, ColumnDesignerViewModel>();
        
        containerRegistry.RegisterForNavigation<MarkedItemsCleanerView, MarkedItemsCleanerViewModel>();
        
        containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}