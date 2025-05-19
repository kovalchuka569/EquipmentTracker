using Prism.Events;
using Syncfusion.UI.Xaml.Maps;

namespace Core.Events.DataGrid.Consumables
{
    public class OnSelectionRecordChanged : PubSubEvent<SelectionRecordChangedEventArgs> {}

    public class SelectionRecordChangedEventArgs
    {
        public int MaterialId { get; set; }
        public string OperationsTableName { get; set; }
        public string TableName { get; set; }
    }
}
