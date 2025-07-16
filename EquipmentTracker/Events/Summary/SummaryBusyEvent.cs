namespace EquipmentTracker.Events.Summary;

public class SummaryBusyEvent : PubSubEvent<SummaryBusyEventArgs> {}

public class SummaryBusyEventArgs : EventArgs
{
    public string Source { get; set; }
    public bool IsBusy { get; set; }
}