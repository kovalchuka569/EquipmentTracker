using System.Collections.ObjectModel;
using System.Windows;

using Syncfusion.Windows.Tools.Controls;
using UI.Views.Tabs.Accounting;
using UI.Views.Tabs.Consumables;
using UI.Views.Tabs.EquipmentTree;
using UI.Views.Tabs.Scheduler;
using UI.Views.Tabs.Settings;

using TabItemExt = Syncfusion.Windows.Tools.Controls.TabItemExt;

public class TabControlExtRegionAdapter : RegionAdapterBase<TabControlExt>
{
    #region Initialize
    private readonly ObservableCollection<TabItemExt> _tabItems = new ObservableCollection<TabItemExt>();
    #endregion

    public TabControlExtRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
        : base(regionBehaviorFactory)
    {
        
    }

    protected override void Adapt(IRegion region, TabControlExt regionTarget)
    {
        //Hide default context menu, because it's in English
        regionTarget.DefaultContextMenuItemVisibility = Visibility.Collapsed;
        regionTarget.CloseButtonType = CloseButtonType.Extended;
        
        //Enabling custom context menu with Localization
        regionTarget.IsCustomTabItemContextMenuEnabled = true;
        
        //Get TabControlExt items source from collection _tabItems
        regionTarget.ItemsSource = _tabItems;
        
        // Handling changes to the presented collection (add/remove)
        region.Views.CollectionChanged += (s, e) =>
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var view in e.NewItems)
                {
                    
                    // Custom context menu items
                    var customContextMenuItems = new ObservableCollection<object>();
                    var viewModel = (view as FrameworkElement)?.DataContext;
                    var tabItem = new TabItemExt
                    {
                        ContextMenuItems = customContextMenuItems,
                        Content = view,
                        Header = GetHeaderFromView(view),
                    };
                    
                    //Adding custom context menu items
                    AddCustomContextMenuItems(tabItem, region, customContextMenuItems);
                    _tabItems.Add(tabItem);
                    regionTarget.Visibility = Visibility.Visible;
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var view in e.OldItems)
                {
                    var itemToRemove = regionTarget.Items.Cast<TabItemExt>().FirstOrDefault(t => t.Content == view);
                    if (itemToRemove != null)
                    {
                        _tabItems.Remove(itemToRemove);
                    }
                }
            }
        };

        // Processing active views
        region.ActiveViews.CollectionChanged += (s, e) =>
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var view in e.NewItems)
                {
                    var tabItem = regionTarget.Items.Cast<TabItemExt>().FirstOrDefault(t => t.Content == view);
                    if (tabItem != null)
                    {
                        regionTarget.SelectedItem = tabItem;
                    }
                }
            }
        };
        
         void AddCustomContextMenuItems(TabItemExt tabItem, IRegion region, ObservableCollection<object> contextMenuItems)
        {
            // Close tab
            CustomMenuItem closeItem = new CustomMenuItem { Header = "Закрити" };
            closeItem.Click += (s, args) =>
            {
                region.Remove(tabItem.Content);
                _tabItems.Remove(tabItem);
            };
            contextMenuItems.Add(closeItem);

            // Close all tabs
            CustomMenuItem closeAllItem = new CustomMenuItem { Header = "Закрити всі" };
            closeAllItem.Click += (s, args) =>
            {
                var itemsToRemove = _tabItems.ToList();
                foreach (var item in itemsToRemove)
                {
                    region.Remove(item.Content);
                    _tabItems.Remove(item);
                }
            };
            contextMenuItems.Add(closeAllItem);

            // Close all tabs but this
            CustomMenuItem closeOthersItem = new CustomMenuItem { Header = "Закрити усе, крім цієї" };
            closeOthersItem.Click += (s, args) =>
            {
                var itemsToRemove = _tabItems.Where(t => t != tabItem).ToList();
                foreach (var item in itemsToRemove)
                {
                    region.Remove(item.Content);
                    _tabItems.Remove(item);
                }
            };
            contextMenuItems.Add(closeOthersItem);
        }
         
         //Event handling for CloseButtonType.Extended;
        regionTarget.TabClosed += (s, e) =>
        {
            var closingTab = e.TargetTabItem;
                region.Remove(closingTab.Content);
                _tabItems.Remove(closingTab);
        };
    }
    
    //Get header from view
    private string GetHeaderFromView(object view)
    {
        return view switch
        {
           EquipmentTreeView => "Виробниче обладнання",
           ToolsTreeView => "Інструменти",
           FurnitureTreeView => "Меблі",
           OfficeTechniqueTreeView => "Офісна техніка",
           ConsumablesView => "Розхідні матеріали",
           AccountingView => "Облік",
           SchedulerView => "Календар",
           SettingsView => "Налаштування",
            _ => view.GetType().Name
        };
    }
    

    protected override IRegion CreateRegion()
    {
        return new SingleActiveRegion();
    }
}