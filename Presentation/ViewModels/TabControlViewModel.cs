using System;
using System.Collections.ObjectModel;
using Core.Events.TabControl;
using Models.TabControl;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Presentation.Interfaces;
using Presentation.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Syncfusion.Windows.Tools.Controls;


namespace Presentation.ViewModels;

public class TabControlViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IGenericTabManager _genericTabManager;
    
    private int _nextTabIndex ;
    
    private ObservableCollection<TabControlItem> _tabItems = new();
    private TabControlItem _selectedTabItem;
    
    public ObservableCollection<TabControlItem> TabItems
    {
        get => _tabItems;
        set => SetProperty(ref _tabItems, value);
    }

    public TabControlItem SelectedTabItem
    {
        get => _selectedTabItem;
        set => SetProperty(ref _selectedTabItem, value);
    }

    public bool AreTabsEmpty => !TabItems.Any();

    public DelegateCommand<TabControlItem> CloseThisCommand { get; }
    public DelegateCommand CloseAllCommand { get; }
    public DelegateCommand<TabControlItem> CloseAllButThisCommand { get; }
    public DelegateCommand<CancelingRoutedEventArgs> TabClosingCommand { get; }
    
    public TabControlViewModel(IEventAggregator eventAggregator, IGenericTabManager genericTabManager)
    {
        _eventAggregator = eventAggregator;
        _genericTabManager = genericTabManager;
        
        CloseThisCommand = new DelegateCommand<TabControlItem>(OnCloseThis);
        CloseAllCommand = new DelegateCommand(OnCloseAll);
        CloseAllButThisCommand = new DelegateCommand<TabControlItem>(OnCloseAllButThis);
        TabClosingCommand = new DelegateCommand<CancelingRoutedEventArgs>(OnTabClosing);
        
        SubscribeToEvents();
        
        TabItems.CollectionChanged += TabItemsOnCollectionChanged;
    }

    private void TabItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(AreTabsEmpty));
    }

    private void OnTabClosing(CancelingRoutedEventArgs args)
    {
        if (args.OriginalSource is TabItemExt tabItem)
        {
            var tabModel = tabItem.Header as TabControlItem ?? tabItem.Content as TabControlItem;
            if (tabModel != null) RemoveTabItem(tabModel);
        }
    }
    


    private void AddNewTab(OpenNewTabEventArgs args)
    {
        var viewModel = _genericTabManager.CreateViewModel(args.Parameters);
        viewModel.InitializeParameters();
        var genericTabView = new GenericTabView { DataContext = viewModel };
        var newTab = new TabControlItem
        {
            Header = args.Header,
            TabIndex = _nextTabIndex++,
            GenericTab = genericTabView,
            CloseThisCommand = CloseThisCommand,
            CloseAllCommand = CloseAllCommand,
            CloseAllButThisCommand = CloseAllButThisCommand
        };
        TabItems.Add(newTab);
    }

    private void OnCloseSelectedTabItem()
    {
        if (SelectedTabItem is TabControlItem tabControlItem)
        {
            RemoveTabItem(tabControlItem);
        }
    }
    
    
    private void OnCloseThis(TabControlItem tabControlItem)
    { 
        RemoveTabItem(tabControlItem);
    }

    private void OnCloseAll()
    {
        var allTabs = TabItems.ToList();
        foreach (var tab in allTabs)
            RemoveTabItem(tab);
    }

    private void OnCloseAllButThis(TabControlItem tabControlItem)
    {
        var itemsToRemove = TabItems.Where(x => x != tabControlItem).ToList();

        foreach (var tab in itemsToRemove)
        {
            RemoveTabItem(tab);
        }

        SelectedTabItem = tabControlItem;
    }

    private void RemoveTabItem(TabControlItem tab)
    {
        if (tab.GenericTab is FrameworkElement fe && fe.DataContext is IDisposable disposable)
            disposable.Dispose();
        TabItems.Remove(tab);
    }

    private void SubscribeToEvents()
    {
        _eventAggregator.GetEvent<OpenNewTabEvent>().Subscribe(AddNewTab);
        _eventAggregator.GetEvent<CloseActiveTabEvent>().Subscribe(OnCloseSelectedTabItem);
    }
}