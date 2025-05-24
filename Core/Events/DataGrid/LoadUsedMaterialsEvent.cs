using Prism.Events;

namespace Core.Events.DataGrid;

public class LoadUsedMaterialsEvent : PubSubEvent<LoadUsedMaterialsEventArgs> {}

public class LoadUsedMaterialsEventArgs
{
    public string RepairsUsedMaterialsTableName {get; set;}
    public int RepairId {get; set;}
}