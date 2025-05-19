using Prism.Events;

namespace Core.Events.TabControl;

public class OpenNewTabEvent : PubSubEvent<OpenNewTabEventArgs> {}

public class OpenNewTabEventArgs
{
    public string Header { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}