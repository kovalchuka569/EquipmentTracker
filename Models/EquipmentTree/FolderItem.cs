using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Models.Enums;
using Prism.Mvvm;

namespace Models.EquipmentTree
{
    public class FolderItem : BindableBase, IFileSystemItem
    {
        private string _name;
        private Guid _id;
        private Guid? _folderId;
        private MenuType _menuType;
        private string _imageIcon;
        private bool _isExpanded;
        private string _nameError;
        private bool _isHighlited;
        private bool _isVisible = true;

        public ObservableCollection<IFileSystemItem> SubItems { get; } = new();

        public ObservableCollection<IFileSystemItem> Children => SubItems;
        public FolderItem()
        {
            _imageIcon = "Assets/folder.png";
        }
        
        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        
        public Guid? FolderId
        {
            get => _folderId;
            set => SetProperty(ref _folderId, value);
        }

        public MenuType MenuType
        {
            get => _menuType;
            set => SetProperty(ref _menuType, value);
        }
        
        public string ImageIcon
        {
            get => _imageIcon;
            set => SetProperty(ref _imageIcon, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
        
        public string NameError
        {
            get => _nameError;
            set => SetProperty(ref _nameError, value);
        }

        public bool IsHighlited
        {
            get => _isHighlited;
            set => SetProperty(ref _isHighlited, value);
        }
        
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
        public bool HaveConnects => true;

        public void AddFile(FileItem file)
        {
            SubItems.Add(file);
        }

        public void AddFolder(FolderItem folder)
        {
            SubItems.Add(folder);
        }


        

        
    }
}

