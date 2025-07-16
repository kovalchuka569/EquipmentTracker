using System.Collections.ObjectModel;
using Core.Models.Tabs.EquipmentTree;
using Models.Equipment;
using Models.EquipmentTree;
using Models.NavDrawer;

namespace Core.Services.EquipmentTree
{
    public interface IEquipmentTreeService
    {
        Task<ObservableCollection<IFileSystemItem>> BuildHierarchy(MenuType menuType);
        Task<int> CreateEquipmentTableAsync();
        Task<int> CreateSummaryAsync(SummaryFormat summaryFormat);
        Task<int> CreateSummaryFileAsync(string name, int folderId, int summaryId, MenuType menuType);
        Task<int> CreateFolderAsync(string name, int? parentId, MenuType menuType);
        Task<int> CreateFileAsync(string name, FileFormat fileFormat, int folderId, int tableId, MenuType menuType);
        Task<string> GenerateUniqueFileFolderNameAsync (string baseName, List<string> existingNames);
        Task RenameFolderAsync(int folderId, string newName);
        Task RenameFileAsync(int fileId, string newName);
    }
}
