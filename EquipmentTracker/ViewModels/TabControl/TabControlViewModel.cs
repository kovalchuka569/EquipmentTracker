using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Events;
using Core.Events.TabControl;
using Models.TabControl;
using UI.ViewModels.TabControl.GenericTab;
using UI.Views.TabControl;
using System.Collections.Specialized;


namespace UI.ViewModels.TabControl;

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
    
    public TabControlViewModel(IEventAggregator eventAggregator, IGenericTabViewModelFactory genericTabViewModelFactory)
    {
        _eventAggregator = eventAggregator;
        _genericTabViewModelFactory = genericTabViewModelFactory;
        
        CloseThisCommand = new DelegateCommand<TabControlItem>(OnCloseThis);
        CloseAllCommand = new DelegateCommand(OnCloseAll);
        CloseAllButThisCommand = new DelegateCommand<TabControlItem>(OnCloseAllButThis);
        
        _eventAggregator.GetEvent<OpenNewTabEvent>().Subscribe(AddNewTab);

        TabItems.CollectionChanged += TabItems_CollectionChanged;
    }

    private void TabItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(AreTabsEmpty));
    }


    private void AddNewTab(OpenNewTabEventArgs args)
    {
        var viewModel = _genericTabViewModelFactory.Create(args.Parameters);
        viewModel.InitializePrameters();
        var genericTabView = new GenericTabView() { DataContext = viewModel };
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
    
    
    private void OnCloseThis(TabControlItem tabControlItem)
    { 
        TabItems.Remove(tabControlItem);
    }

    private void OnCloseAll()
    {
        TabItems.Clear();
    }

    private void OnCloseAllButThis(TabControlItem tabControlItem)
    {
        var itemsToRemove = TabItems.Where(x => x != tabControlItem).ToList();

        foreach (var tab in itemsToRemove)
        {
            TabItems.Remove(tab);
        }

        SelectedTabItem = tabControlItem;
    }
}