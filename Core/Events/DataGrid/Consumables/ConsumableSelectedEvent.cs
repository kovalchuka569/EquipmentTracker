using Models.ConsumablesDataGrid;
using Prism.Events;

namespace Core.Events.DataGrid.Consumables;

public class ConsumableSelectedEvent : PubSubEvent<ConsumableSelectedEventArgs> {}

public class ConsumableSelectedEventArgs
{
    public string ConsumableTableName { get; set; }
    public ConsumableItem ConsumableItem { get; set; }
}