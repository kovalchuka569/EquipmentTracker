using System.Collections.ObjectModel;
using Core.Models.Tabs.EquipmentTree;
using Models.Equipment;
using Models.EquipmentTree;

namespace Core.Services.EquipmentTree
{
    public interface IEquipmentTreeService
    {
        Task<ObservableCollection<IFileSystemItem>> BuildHierarchy(string menuType);
        Task<int> InsertFolderAsync (string name, int? parentId, string menuType);
        Task<int> InsertFileAsync(string name, int folderId, string menuType);
        Task<string> GenerateUniqueFileFolderNameAsync (string baseFolderName, List<string> existingNames);
        Task RenameFolderAsync(int folderId, string newName);
        Task RenameChildsAsync(int folderId, string newName, string oldName, string menuType);
    }
}
