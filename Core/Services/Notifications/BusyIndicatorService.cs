

using Core.Events;

namespace Core.Services.Notifications;

public class BusyIndicatorService
{
    private readonly IEventAggregator _eventAggregator;

    public BusyIndicatorService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }
    public void HiddenBusyIndicator()
    {
        _eventAggregator.GetEvent<BusyIndicatorVisibilityIndicatorEvent>().Publish("Hidden");
    }
    public void VisibleBusyIndicator()
    {
        _eventAggregator.GetEvent<BusyIndicatorVisibilityIndicatorEvent>().Publish("Visible");
    }
    
    public void StartBusy()
    {
        _eventAggregator.GetEvent<BusyIndicatorIsBusyEvent>().Publish(true);
    }
    public void StopBusy()
    {
        _eventAggregator.GetEvent<BusyIndicatorIsBusyEvent>().Publish(false);
    }
    
    public void EmptyMessage()
    {
        _eventAggregator.GetEvent<BusyIndicatorMessageEvent>().Publish(string.Empty);
    }
    public void DefaultAuthMessage()
    {
        _eventAggregator.GetEvent<BusyIndicatorMessageEvent>().Publish("Авторизація...");
    }
    public void ErrorAuthMessage()
    {
        _eventAggregator.GetEvent<BusyIndicatorMessageEvent>().Publish("Помилка авторизації...");
    }
    public void SuccessAuthMessage()
    {
        _eventAggregator.GetEvent<BusyIndicatorMessageEvent>().Publish("Вхід...");
    }
}