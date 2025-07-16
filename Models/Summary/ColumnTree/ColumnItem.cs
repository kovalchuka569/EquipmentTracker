using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Models.Summary.ColumnTree;

public class ColumnItem : BindableBase, ISummaryFileSystemItem
{
    private int _parentFileId;
    private int _columnId;
    private string _name;
    private string _imageIcon;
    private ObservableCollection<ISummaryFileSystemItem> _children;
    private bool? _isSelected;
    public SolidColorBrush Foreground => IsSelected ?? false ? Brushes.Green : Brushes.Black;

    
    public int ParentFileId
    {
        get => _parentFileId;
        set => SetProperty(ref _parentFileId, value);
    }

    public int ColumnId
    {
        get => _columnId;
        set => SetProperty(ref _columnId, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ObservableCollection<ISummaryFileSystemItem> Children
    {
        get => _children;
        set => SetProperty(ref _children, value);
    }

    public string ImageIcon
    {
        get => _imageIcon;
        set => SetProperty(ref _imageIcon, value);
    }

    public bool? IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                RaisePropertyChanged(nameof(Foreground));
            }
        }
    }
}