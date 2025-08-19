using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Models.Enums;
using Prism.Mvvm;

namespace Models.EquipmentTree
{
    public class FileItem : BindableBase, IFileSystemItem
    {
        private Guid _id;
        private string _name;
        private Guid? _folderId;
        private string _imageIcon;
        private FileFormat _fileFormat;
        private MenuType _menuType;
        private Guid _summaryId;
        private Guid _equipmentSheetId;
        private bool _isHighlited;
        private bool _isVisible = true;

        private Guid? _tableId;
        private Dictionary<string, int> _connections = new();

        public ObservableCollection<IFileSystemItem> Children { get; } = new();
        
        public bool HaveConnects => FileFormat == FileFormat.EquipmentSheet || FileFormat == FileFormat.SummaryEquipment 
                                                                            || FileFormat == FileFormat.SummaryRepairs 
                                                                            || FileFormat == FileFormat.SummaryServices 
                                                                            || FileFormat == FileFormat.SummaryWriteOff 
                                                                            || Connections.Any();
        
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
        
        public bool IsExpanded { get; set; }

        public Guid? FolderId
        {
            get => _folderId;
            set => SetProperty(ref _folderId, value);
        }

        public Dictionary<string, int> Connections
        {
            get => _connections;
            set
            {
                if (SetProperty(ref _connections, value))
                    RaisePropertyChanged(nameof(HaveConnects));
            }
        }

        public string ImageIcon => _fileFormat switch
        {
            FileFormat.EquipmentSheet or FileFormat.RepairsSheet or FileFormat.ServicesSheet or FileFormat.WriteOffSheet => "../Resources/Icons/TableSheet/tablesheet_dark_line_24.png",
            FileFormat.SummaryEquipment or FileFormat.SummaryRepairs or FileFormat.SummaryServices or FileFormat.SummaryWriteOff  => "Assets/summary.png",
            _ => String.Empty
        };

        public FileFormat FileFormat
        {
            get => _fileFormat;
            set => SetProperty(ref _fileFormat, value);
        }

        public MenuType MenuType
        {
            get => _menuType;
            set => SetProperty(ref _menuType, value);
        }

        public Guid SummaryId
        {
            get => _summaryId;
            set => SetProperty(ref _summaryId, value);
        }

        public Guid EquipmentSheetId
        {
            get => _equipmentSheetId;
            set => SetProperty(ref _equipmentSheetId, value);
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

        public Guid? TableId
        {
            get => _tableId;
            set => SetProperty(ref _tableId, value);
        }
        

    }
}
