using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Models.Consumables
{
    public class File : IFileSystemItem, INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private int _parentIdFolder;
        private string _imageIcon;
        private int _badgeValue;
        private bool _badgeVisibility;
        
        public File()
        {
            _imageIcon = "Assets/file.png";
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
        
        public int ParentIdFolder
        {
            get => _parentIdFolder;
            set
            {
                _parentIdFolder = value;
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
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
