using System.Windows;
using Common;
using UI.Modules;
using Microsoft.Extensions.Logging;
using Common.Logging;
using Core;
using Data;
using LocalDbConnectionService;
using LocalDbConnectionService.KeyManagers;
using Presentation;
using Presentation.Views;
using UI.ViewModels.Updater;
using UI.Views.Updater;

namespace EquipmentTracker;

public partial class App
{

    public App()
    {
        Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("uk-UA");
        
        SyncfusionKeyManager.RegisterSyncfusionKey();
    }
    
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));
        containerRegistry.RegisterSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
        containerRegistry.RegisterForNavigation<UpdaterView, UpdaterViewModel>();
    }
    
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<LocalSecureModule>();
        moduleCatalog.AddModule<CommonModule>();
        moduleCatalog.AddModule<PresentationModule>();
        moduleCatalog.AddModule<CoreModule>();
        moduleCatalog.AddModule<DataModule>();
        moduleCatalog.AddModule<AuthModule>();
        moduleCatalog.AddModule<TabControlModule>();
    }

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindowView>();
    }
}