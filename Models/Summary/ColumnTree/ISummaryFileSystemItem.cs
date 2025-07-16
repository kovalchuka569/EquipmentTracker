using System.Collections.ObjectModel;

namespace Models.Summary.ColumnTree;

public interface ISummaryFileSystemItem
{
    string Name { get; set; }
    ObservableCollection<ISummaryFileSystemItem> Children { get; }
    string ImageIcon { get; set; }
    public bool? IsSelected { get; set; }
}