using Prism.Events;

namespace Core.Events.DataGrid;

public class IsAnyNullQuantityConsumables : PubSubEvent<Action<bool>> {}
