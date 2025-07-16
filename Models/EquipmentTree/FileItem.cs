using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Prism.Mvvm;

namespace Models.EquipmentTree
{
    public class FileItem : BindableBase, IFileSystemItem
    {
        private int _id;
        private string _name;
        private int _parentIdFolder;
        private string _imageIcon;
        private FileFormat _fileFormat;
        private int? _summaryId;
        private bool _isHighlited;
        private bool _isVisible = true;

        private int? _tableId;
        private Dictionary<string, int> _connections = new();

        public ObservableCollection<IFileSystemItem> Children { get; } = new();
        
        // Показывает наличие связей: для Equipments всегда true, иначе зависит от Connections.
        public bool HaveConnects => FileFormat == FileFormat.EquipmentSheet || FileFormat == FileFormat.Summary || Connections.Any();
        
        public int Id
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

        public int ParentIdFolder
        {
            get => _parentIdFolder;
            set => SetProperty(ref _parentIdFolder, value);
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
            FileFormat.EquipmentSheet or FileFormat.RepairsSheet or FileFormat.ServicesSheet or FileFormat.WriteOffSheet => "Assets/file.png",
            FileFormat.Summary => "Assets/summary.png",
            _ => String.Empty
        };

        public FileFormat FileFormat
        {
            get => _fileFormat;
            set => SetProperty(ref _fileFormat, value);
        }

        public int? SummaryId
        {
            get => _summaryId;
            set => SetProperty(ref _summaryId, value);
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

        public int? TableId
        {
            get => _tableId;
            set => SetProperty(ref _tableId, value);
        }
        

    }
}
