using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Models.Tabs.ProductionEquipmentTree;

public class Folder : INotifyPropertyChanged
{
    private int _id;
    private string _fileName;
    private bool _isExpanded;
    private string _imageIcon;
    private ObservableCollection<Folder> _subFolders;
    
    public event PropertyChangedEventHandler PropertyChanged;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged();
        }
    }

    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            OnPropertyChanged();
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    public string ImageIcon
    {
        get => _imageIcon;
        set => _imageIcon = value;
    }

    public ObservableCollection<Folder> SubFolders
    {
        get => _subFolders;
        set
        {
            _subFolders = value;
            OnPropertyChanged();
        }
    }


    public Folder()
    {
        SubFolders = new ObservableCollection<Folder>();
        ImageIcon = "Assets/folder.png";
        IsExpanded = false;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}