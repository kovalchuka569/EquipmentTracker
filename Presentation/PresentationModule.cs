using Prism.Ioc;
using Prism.Modularity;

using Presentation.ViewModels;
using Presentation.Views;

using Presentation.Interfaces;
using Presentation.Services;
using Presentation.Services.Interfaces;
using Presentation.ViewModels.DialogViewModels;
using Presentation.Views.DialogViews;

namespace Presentation;

public class PresentationModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Interfaces and UI services
        containerRegistry.RegisterScoped<ISnackbarService, SnackbarService>();
        containerRegistry.RegisterScoped<IOverlayService, OverlayService>();
        containerRegistry.RegisterScoped<IBusyIndicatorService, BusyIndicatorService>();
        containerRegistry.RegisterScoped<IDialogService, DialogService>();
        
        // Interfaces and managers
        containerRegistry.RegisterScoped<IGenericTabManager, GenericTabManager>();
        containerRegistry.RegisterScoped<IExcelExportManager, ExcelExportManager>();
        containerRegistry.RegisterScoped<IPdfExportManager, PdfExportManager>();
        containerRegistry.RegisterScoped<ISyncfusionGridPrintManager, SyncfusionGridPrintManager>();
        containerRegistry.RegisterScoped<ISyncfusionGridColumnManager, SyncfusionGridColumnManager>();
        containerRegistry.RegisterScoped<IGridInteractionManager, GridInteractionManager>();
        containerRegistry.RegisterScoped<ISfDataGridExportManager, SfDataGridExportManager>();
        
        // Views and view models
        containerRegistry.RegisterForNavigation<SnackbarView, SnackbarViewModel>();
        containerRegistry.RegisterForNavigation<MainWindowView, MainWindowViewModel>();
        containerRegistry.RegisterForNavigation<GenericTabView, GenericTabViewModel>();
        containerRegistry.RegisterForNavigation<AuthorizationView, AuthorizationViewModel>();
        containerRegistry.RegisterForNavigation<NavDrawerView, NavDrawerViewModel>();
        containerRegistry.RegisterForNavigation<MainTreeView, MainTreeViewModel>();
        containerRegistry.RegisterForNavigation<EquipmentSheetView, EquipmentSheetViewModel>();
        containerRegistry.RegisterForNavigation<PivotSheetView, PivotSheetViewModel>();
        containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
        containerRegistry.RegisterForNavigation<UserManagerView, UserManagerViewModel>();
        
        // Dialogs
        containerRegistry.RegisterForNavigation<DialogBoxView, DialogBoxViewModel>();
        containerRegistry.RegisterForNavigation<ExcelImportConfiguratorView, ExcelImportConfiguratorViewModel>();
        containerRegistry.RegisterForNavigation<ColumnDesignerView, ColumnDesignerViewModel>();
        containerRegistry.RegisterForNavigation<MarkedItemsCleanerView, MarkedItemsCleanerViewModel>();
        containerRegistry.RegisterForNavigation<ConnectionFailedView, ConnectionFailedViewModel>();
        containerRegistry.RegisterForNavigation<ConnectionSetupView, ConnectionSetupViewModel>();
        containerRegistry.RegisterForNavigation<RegisterView, RegisterViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}