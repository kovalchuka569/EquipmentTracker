using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Models.EquipmentTree
{
    public class FolderItem : IFileSystemItem,  INotifyPropertyChanged
    {
        private string _name;
        private int _id;
        private int? _parentId;
        private string _imageIcon;
        private bool _isExpanded;
        private string _nameError;

        public ObservableCollection<IFileSystemItem> SubItems { get; } = new();
        public FolderItem()
        {
            _imageIcon = "Assets/folder.png";
        }
        
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        
        public int? ParentId
        {
            get => _parentId;
            set
            {
                _parentId = value;
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
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
        
        public string NameError
        {
            get => _nameError;
            set
            {
                _nameError = value;
                OnPropertyChanged();
            }
        }
        
        public void AddFile(FileItem file)
        {
            SubItems.Add(file);
        }

        public void AddFolder(FolderItem folder)
        {
            SubItems.Add(folder);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}

