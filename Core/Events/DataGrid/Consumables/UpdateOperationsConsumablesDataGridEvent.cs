using Prism.Events;

namespace Core.Events.DataGrid.Consumables
{
    public class UpdateOperationsConsumablesDataGridEvent : PubSubEvent<UpdateOperationsConsumablesDataGridEventArgs>{}

    public class UpdateOperationsConsumablesDataGridEventArgs
    {
        public int MaterialId { get; set; }
        public string OperationsTableName { get; set; }
        public string TableName { get; set; }
    }
}
