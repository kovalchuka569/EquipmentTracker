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
    private ObservableCollection<File> _files;
    
    public event PropertyChangedEventHandler? PropertyChanged;

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
    public IEnumerable<object> Items => SubFolders.Cast<object>().Concat(Files);
    public ObservableCollection<Folder> SubFolders{ get; set; } = new();

    public ObservableCollection<File> Files{ get; set; } = new();

    public void AddFile(File file)
    {
        Files.Add(file);
        OnPropertyChanged(nameof(Items));
    }
    public void AddFolder (Folder folder)
    {
        SubFolders.Add(folder);
        OnPropertyChanged(nameof(Items));
    }



    public Folder()
    {
        _subFolders = new ObservableCollection<Folder>();
        _files = new ObservableCollection<File>();
        _imageIcon = "Assets/folder.png";
        _isExpanded = false;
    }


    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}