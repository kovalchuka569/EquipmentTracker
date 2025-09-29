using Prism.Ioc;
using Prism.Modularity;

using Presentation.ViewModels;
using Presentation.Views;

using Presentation.Interfaces;
using Presentation.UIManagers;
using Presentation.ViewModels.DialogViewModels;
using Presentation.Views.DialogViews;

namespace Presentation;

public class PresentationModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Interfaces and UIManagers
        containerRegistry.RegisterScoped<IGenericTabManager, GenericTabManager>();
        containerRegistry.RegisterScoped<ICleanRegionManager, CleanRegionManager>();
        containerRegistry.RegisterScoped<IExcelExportManager, ExcelExportManager>();
        containerRegistry.RegisterScoped<IPdfExportManager, PdfExportManager>();
        containerRegistry.RegisterScoped<ISyncfusionGridPrintManager, SyncfusionGridPrintManager>();
        containerRegistry.RegisterScoped<ISyncfusionGridColumnManager, SyncfusionGridColumnManager>();
        containerRegistry.RegisterScoped<IDialogManager, DialogManager>();
        containerRegistry.RegisterScoped<IGridInteractionManager, GridInteractionManager>();
        containerRegistry.RegisterScoped<IOverlayManager, OverlayManager>();
        containerRegistry.RegisterScoped<IBusyIndicatorManager, BusyIndicatorManager>();
        
        // Views and view models
        containerRegistry.RegisterForNavigation<MainWindowView, MainWindowViewModel>();
        containerRegistry.RegisterForNavigation<GenericTabView, GenericTabViewModel>();
        containerRegistry.RegisterForNavigation<AuthorizationView, AuthorizationViewModel>();
        containerRegistry.RegisterForNavigation<NavDrawerView, NavDrawerViewModel>();
        containerRegistry.RegisterForNavigation<MainTreeView, MainTreeViewModel>();
        containerRegistry.RegisterForNavigation<EquipmentSheetView, EquipmentSheetViewModel>();
        containerRegistry.RegisterForNavigation<PivotSheetView, PivotSheetViewModel>();
        containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
        
        // Dialogs
        containerRegistry.RegisterForNavigation<DialogBoxView, DialogBoxViewModel>();
        containerRegistry.RegisterForNavigation<ExcelImportConfiguratorView, ExcelImportConfiguratorViewModel>();
        containerRegistry.RegisterForNavigation<ColumnDesignerView, ColumnDesignerViewModel>();
        containerRegistry.RegisterForNavigation<MarkedItemsCleanerView, MarkedItemsCleanerViewModel>();
        containerRegistry.RegisterForNavigation<ConnectionFailedView, ConnectionFailedViewModel>();
        containerRegistry.RegisterForNavigation<ConnectionSetupView, ConnectionSetupViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}