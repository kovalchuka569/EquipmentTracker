using System.Collections.ObjectModel;

namespace Models.Summary.ColumnTree;

public class FolderItem : BindableBase, ISummaryFileSystemItem
{
    private int _id;
    private int _parentId;
    private string _name;
    private ObservableCollection<ISummaryFileSystemItem> _children;
    private bool _isExpanded;
    private string _imageIcon;
    private bool? _isSelected;

    public bool HaveChilds => Children.Any();
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    public int ParentId
    {
        get => _parentId;
        set => SetProperty(ref _parentId, value);
    }
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    public ObservableCollection<ISummaryFileSystemItem> Children
    {
        get => _children;
        set
        {
            if (SetProperty(ref _children, value))
            {
                RaisePropertyChanged(nameof(HaveChilds));
            }
        }
    }
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
    public string ImageIcon
    {
        get => _imageIcon;
        set => SetProperty(ref _imageIcon, value);
    }

    public bool? IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}