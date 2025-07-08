
namespace UI.ViewModels.Notifications.BusyIndicator;

public class BusyIndicatorViewModel : BindableBase
{
    private bool _isBusy;
    private string _message;
    private string _visibilityIndicator;
    private readonly IEventAggregator _eventAggregator;

    public BusyIndicatorViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _eventAggregator.GetEvent<Core.Events.BusyIndicatorIsBusyEvent>().Subscribe(SetBusy);
        _eventAggregator.GetEvent<Core.Events.BusyIndicatorMessageEvent>().Subscribe(SetMessage);
        _eventAggregator.GetEvent<Core.Events.BusyIndicatorVisibilityIndicatorEvent>().Subscribe(SetVisibilityIndicator);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string VisibilityIndicator
    {
        get => _visibilityIndicator;
        set => SetProperty(ref _visibilityIndicator, value);
    }
    
    private void SetBusy(bool isBusy)
    {
        IsBusy = isBusy;
    }

    private void SetMessage(string message)
    {
        Message = message;
    }

    private void SetVisibilityIndicator(string visibilityIndicator)
    {
        VisibilityIndicator = visibilityIndicator;
    }
}