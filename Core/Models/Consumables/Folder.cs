using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Models.Consumables
{
    public class Folder : IFileSystemItem, INotifyPropertyChanged
    {
        private string _name;
        private int _id;
        private int? _parentId;
        private string _imageIcon;
        private bool _isExpanded;
        private int _badgeValue;
        private bool _badgeVisibility;
        
        public ObservableCollection<IFileSystemItem> Items { get; } = new();
        public Folder()
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

        public int BadgeValue
        {
            get => _badgeValue;
            set
            {
                _badgeValue = value;
                OnPropertyChanged();
            }
        }

        public bool BadgeVisibility
        {
            get => _badgeVisibility;
            set
            {
                _badgeVisibility = value;
                OnPropertyChanged();
            }
        }

        public void AddFile(File file)
        {
            Items.Add(file);
        }

        public void AddFolder(Folder folder)
        {
            Items.Add(folder);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

