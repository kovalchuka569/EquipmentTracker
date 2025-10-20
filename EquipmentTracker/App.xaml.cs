using System.Globalization;
using System.Windows;
using Common;
using Microsoft.Extensions.Logging;
using Common.Logging;
using Core;
using Data;
using LocalSecure;
using LocalSecure.KeyManagers;
using Presentation;
using Presentation.Views;
using Syncfusion.Licensing;
using UI.ViewModels.Updater;
using UI.Views.Updater;

namespace EquipmentTracker;

public partial class App
{

    public App()
    {
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("uk-UA"); 
        SyncfusionLicenseProvider.RegisterLicense(SyncfusionKeyManager.SyncfusionLicenseKey);
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
        moduleCatalog.AddModule<DataModule>();
        moduleCatalog.AddModule<CommonModule>();
        moduleCatalog.AddModule<CoreModule>();
        moduleCatalog.AddModule<PresentationModule>();
    }

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindowView>();
    }
}