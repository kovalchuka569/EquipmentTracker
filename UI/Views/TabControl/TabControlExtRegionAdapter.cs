using System.Collections.ObjectModel;
using System.Windows;
using Prism.Navigation.Regions;
using Syncfusion.UI.Xaml.Diagram.Stencil;
using Syncfusion.Windows.Controls.Navigation;
using Syncfusion.Windows.Tools.Controls;
using UI.Views.Tabs.Accounting;
using UI.Views.Tabs.Consumables;
using UI.Views.Tabs.Furniture;
using UI.Views.Tabs.OfficeTechnique;
using UI.Views.Tabs.ProductionEquipmentTree;
using UI.Views.Tabs.Scheduler;
using UI.Views.Tabs.Settings;
using UI.Views.Tabs.ToolsTree;

public class TabControlExtRegionAdapter : RegionAdapterBase<TabControlExt>
{
    private readonly ObservableCollection<TabItemExt> _tabItems = new ObservableCollection<TabItemExt>();

    public TabControlExtRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
        : base(regionBehaviorFactory)
    {
        
    }

    protected override void Adapt(IRegion region, TabControlExt regionTarget)
    {
        Console.WriteLine($"Adapt called. Tab count: {regionTarget.Items.Count}, Visible: {regionTarget.Visibility}");
        region.Views.CollectionChanged += (s, e) =>
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var view in e.NewItems)
                {
                    var tabItem = new TabItemExt
                    {
                        Header = GetHeaderFromView(view),
                        Content = view,
                    };
                    
                    
                    regionTarget.Items.Add(tabItem);
                    regionTarget.Visibility = Visibility.Visible;
                    Console.WriteLine($"Add tab: {tabItem.Header}, Tab count: {regionTarget.Items.Count}, Visible: {regionTarget.Visibility}");
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var view in e.OldItems)
                {
                    var itemToRemove = regionTarget.Items.Cast<TabItemExt>().FirstOrDefault(t => t.Content == view);
                    if (itemToRemove != null)
                    {
                        regionTarget.Items.Remove(itemToRemove);
                        Console.WriteLine($"Removed tab: {itemToRemove.Header}, Tab count: {regionTarget.Items.Count}, Visible: {regionTarget.Visibility}");

                    }
                }
            }
        };

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
                        Console.WriteLine($"Activated tab: {tabItem.Header}, Tab count: {regionTarget.Items.Count}, Visible: {regionTarget.Visibility}");
                    }
                }
            }
        };
    }
    
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