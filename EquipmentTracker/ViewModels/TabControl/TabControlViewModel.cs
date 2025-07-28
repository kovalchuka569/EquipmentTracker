using System.Collections.ObjectModel;
using Core.Events.TabControl;
using Models.TabControl;
using UI.ViewModels.TabControl.GenericTab;
using UI.Views.TabControl;
using System.Collections.Specialized;
using System.Windows;
using Syncfusion.Windows.Tools.Controls;


namespace EquipmentTracker.ViewModels.TabControl;

public class TabControlViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IGenericTabViewModelFactory _genericTabViewModelFactory;
    
    private int _nextTabIndex = 0;


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
    
    public TabControlViewModel(IEventAggregator eventAggregator, IGenericTabViewModelFactory genericTabViewModelFactory)
    {
        _eventAggregator = eventAggregator;
        _genericTabViewModelFactory = genericTabViewModelFactory;
        
        CloseThisCommand = new DelegateCommand<TabControlItem>(OnCloseThis);
        CloseAllCommand = new DelegateCommand(OnCloseAll);
        CloseAllButThisCommand = new DelegateCommand<TabControlItem>(OnCloseAllButThis);
        TabClosingCommand = new DelegateCommand<CancelingRoutedEventArgs>(OnTabClosing);
        
        _eventAggregator.GetEvent<OpenNewTabEvent>().Subscribe(AddNewTab);
        _eventAggregator.GetEvent<CloseActiveTabEvent>().Subscribe(OnCloseSelectedTabItem);
        
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
        var viewModel = _genericTabViewModelFactory.Create(args.Parameters);
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
        SelectedTabItem = newTab;
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
}