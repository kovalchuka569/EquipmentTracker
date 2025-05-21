using Prism.Events;

namespace Core.Events.DataGrid;

public class IsEmptyUsedMaterials : PubSubEvent<Action<bool>> {}