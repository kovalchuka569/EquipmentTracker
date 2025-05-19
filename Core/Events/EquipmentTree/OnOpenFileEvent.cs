using Prism.Events;

namespace Core.Events.EquipmentTree
{
    public class OnOpenFileEvent : PubSubEvent<OnOpenFileEventArgs> { }

    public class OnOpenFileEventArgs
    {
        public string FileName { get; set; }
        public string MenuType { get; set; }
        public string TableName { get; set; }
        public string FileType { get; set; }
    }
}
