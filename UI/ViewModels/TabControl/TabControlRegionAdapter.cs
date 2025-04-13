using System.Windows;
using Syncfusion.Windows.Tools.Controls;

namespace UI.ViewModels.TabControl;

public class TabControlRegionAdapter : RegionAdapterBase<TabControlExt>
    {
        private TabControlExt _tabControl;
        private readonly Dictionary<string, TabItemExt> _parameterToTabMap = new Dictionary<string, TabItemExt>();

        public TabControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, TabControlExt regionTarget)
        {
            _tabControl = regionTarget;
            
            // Prevent content from reloading when switching tabs
            _tabControl.SelectionChanged += (sender, e) =>
            {
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItemExt newTab)
                {
                    Console.WriteLine($"Активирована вкладка: {newTab.Header}");
                    // Activate the view in the region
                    if (newTab.Content is FrameworkElement content)
                    {
                        region.Activate(content);
                    }
                }
            };

            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (FrameworkElement element in e.NewItems)
                    {
                        // Create a new tab
                        TabItemExt item;
                        
                        if (element.DataContext is GenericTabViewModel newVm)
                        {
                            Console.WriteLine($"Adding view with GenericTabViewModel: {element.GetType().Name}");
                            
                            // Create a new tab with a temporary title
                            item = new TabItemExt
                            {
                                Content = element,
                                Header = element.GetType().Name.Replace("View", "")
                            };
                            
                            // If TabName is already set, use it for the title
                            if (!string.IsNullOrEmpty(newVm.TabName))
                            {
                                item.Header = newVm.TabName;
                                // Add to the parameter map
                                _parameterToTabMap[newVm.TabName] = item;
                                Console.WriteLine($"Using initial TabName: {newVm.TabName}");
                            }

                            // Subscribe to change of TabName property
                            newVm.PropertyChanged += (sender, args) =>
                            {
                                if (args.PropertyName == nameof(GenericTabViewModel.TabName) &&
                                    !string.IsNullOrEmpty(newVm.TabName))
                                {
                                    string parameter = newVm.TabName;
                                    Console.WriteLine($"TabName changed to: {parameter}");
                                    
                                    // Check if there is already a tab with such a parameter in our map
                                    if (_parameterToTabMap.TryGetValue(parameter, out var existingTab) && 
                                        existingTab != item && existingTab.Parent == _tabControl)
                                    {
                                        Console.WriteLine($"Found existing tab with parameter: {parameter}, merging");
                                        
                                        // If find an existing tab, delete the current one
                                        if (_tabControl.Items.Contains(item))
                                        {
                                            _tabControl.Items.Remove(item);
                                            _tabControl.SelectedItem = existingTab;
                                            
                                            // Remove the view from the region (only if it is not equal to existingTab.Content)
                                            if (region.Views.Contains(element) && element != existingTab.Content)
                                            {
                                                region.Remove(element);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Updating the title
                                        item.Header = parameter;
                                        // Add/update in the parameter map
                                        _parameterToTabMap[parameter] = item;
                                    }
                                }
                            };
                        }
                        else
                        {
                            // For views without GenericTabViewModel we use standard logic
                            item = new TabItemExt
                            {
                                Header = element.GetType().Name.Replace("View", ""),
                                Content = element
                            };
                        }

                        // Add a tab and activate it
                        regionTarget.Items.Add(item);
                        regionTarget.SelectedItem = item;
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (FrameworkElement element in e.OldItems)
                    {
                        var tabItem = regionTarget.Items.Cast<TabItemExt>()
                            .FirstOrDefault(item => item.Content == element);
                        if (tabItem != null)
                        {
                            // Remove from the parameter map
                            if (element.DataContext is GenericTabViewModel vm && !string.IsNullOrEmpty(vm.TabName))
                            {
                                if (_parameterToTabMap.TryGetValue(vm.TabName, out var mappedTab) && mappedTab == tabItem)
                                {
                                    _parameterToTabMap.Remove(vm.TabName);
                                }
                            }

                            regionTarget.Items.Remove(tabItem);
                        }
                    }
                }
            };

            regionTarget.OnCloseButtonClick += (s, e) =>
            {
                if (e.TargetTabItem?.Content is FrameworkElement content)
                {
                    // Remove from the parameter map before removing from the region
                    if (content.DataContext is GenericTabViewModel vm && !string.IsNullOrEmpty(vm.TabName))
                    {
                        if (_parameterToTabMap.TryGetValue(vm.TabName, out var mappedTab) && 
                            mappedTab == e.TargetTabItem)
                        {
                            _parameterToTabMap.Remove(vm.TabName);
                        }
                    }

                    region.Remove(content);
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new SingleActiveRegion();
        }
    }