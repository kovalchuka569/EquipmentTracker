using System.Collections.ObjectModel;

namespace Models.EquipmentTree
{
    public interface IFileSystemItem
    {
        string Name { get; set; }
        ObservableCollection<IFileSystemItem> Children { get; }
        bool IsExpanded { get; set; }
        bool IsVisible { get; set; }
    }
}
