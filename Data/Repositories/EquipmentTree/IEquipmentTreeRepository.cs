using Models.EquipmentTree;

namespace Data.Repositories.EquipmentTree
{
    public interface IEquipmentTreeRepository
    {
        Task<List<FolderDto>> GetFoldersAsync(string menuType);
        Task<List<FileDto>> GetFilesAsync(string menuType);
        Task<int> InsertFolderAsync(string name, int? parentId, string menuType);
        Task<int> InsertFileAsync(string name, int folderId, string menuType);
        Task RenameFolderAsync(int folderId, string newName);
        Task RenameChildsAsync(int folderId, string newName, string oldName, string menuType);
    }
}
