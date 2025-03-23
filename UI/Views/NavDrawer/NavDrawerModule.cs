using Core.Models.Auth;
using Core.Services.TreeView;
using UI.Views.Auth;
using UI.Views.TabControl;
using UI.Views.Tabs.ProductionEquipmentTree;
using UI.Views.Tabs.Scheduler;
using UI.Views.Tabs.Settings;

namespace UI.Views.NavDrawer;

 class NavDrawerModule: IModule
{
 public void OnInitialized(IContainerProvider containerProvider)
 {
 }

 public void RegisterTypes(IContainerRegistry containerRegistry)
 {
  containerRegistry.RegisterForNavigation<TabControlView>();
 }


}