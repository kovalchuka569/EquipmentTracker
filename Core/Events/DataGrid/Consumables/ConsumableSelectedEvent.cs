using Models.ConsumablesDataGrid;
using Prism.Events;

namespace Core.Events.DataGrid.Consumables;

public class ConsumableSelectedEvent : PubSubEvent<ConsumableItem> {}