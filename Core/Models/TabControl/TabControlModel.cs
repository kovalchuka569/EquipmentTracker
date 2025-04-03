using System.Windows.Controls;

namespace Core.Models.TabControl;

public class TabControlModel
{
    public string Header { get; set; }
    public string ViewName { get; set; }
}

public class CreateTabFromFileEvent : PubSubEvent<TabControlModel>
{
    
}