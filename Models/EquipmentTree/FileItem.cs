using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Models.EquipmentTree
{
    public class FileItem : IFileSystemItem, INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private int _parentIdFolder;
        private string _imageIcon;
        private string _tableName;
        private string _fileType;

        public FileItem()
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

        public string TableName
        {
            get => _tableName;
            set
            {
                _tableName = value;
                OnPropertyChanged();
            }
        }

        public string FileType
        {
            get => _fileType;
            set
            {
                _fileType = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
