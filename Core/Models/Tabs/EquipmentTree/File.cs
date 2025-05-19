using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Models.Tabs.EquipmentTree
{
    public class File : IFileSystem, INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private int _parentIdFolder;
        private string _imageIcon;
        
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
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
