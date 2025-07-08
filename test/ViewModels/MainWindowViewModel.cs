using Prism.Commands;

using test.Events;
using test.Views;

namespace test.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRegionManager _regionManager;
    
    public DelegateCommand Open1TabCommand { get; }
    public DelegateCommand Open2TabCommand { get; }
    public DelegateCommand Open3TabCommand { get; }
    public DelegateCommand Open4TabCommand { get; }
    
    
    public MainWindowViewModel(IEventAggregator eventAggregator, 
        IRegionManager regionManager)
    {
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        
        Open1TabCommand = new DelegateCommand(OnOpenViewA);
        Open2TabCommand = new DelegateCommand(OnOpenViewB);
        Open3TabCommand = new DelegateCommand(OnOpenViewA);
        Open4TabCommand = new DelegateCommand(OnOpenViewB);
    }

    private void OnOpenViewA()
    {
        _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
        {
            Header = "ViewATab",
            Parameters = new Dictionary<string, object>
            {
                {"ViewNameToShow", "ViewA"},
                {"ViewA.SomeText", "Some Text For ViewA"}
            }
        });
    }

    private void OnOpenViewB()
    {
        _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
        {
            Header = "ViewBTab",
            Parameters = new Dictionary<string, object>
            {
                {"ViewNameToShow", "ViewB"},
                {"ViewB.SomeText", "Some Text For ViewB"}
            }
        });
    }
}