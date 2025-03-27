using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Models.Tabs.ProductionEquipmentTree;

public class File : INotifyPropertyChanged
{
    private int _id;
    private string _fileName;
    private string _imageIcon;
    private int _folderId;

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

    public string ImageIcon
    {
        get => _imageIcon;
        set
        {
            _imageIcon = value;
            OnPropertyChanged();
        }
    }

    public int FolderId
    {
        get => _folderId;
        set
        {
            _folderId = value;
            OnPropertyChanged();
        }
    }

    public File()
    {
        ImageIcon = "Assets/file.png";
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}