using Models.Equipment;
using Models.EquipmentTree;
using Models.NavDrawer;

namespace Data.Repositories.EquipmentTree
{
    public interface IEquipmentTreeRepository
    {
        Task<List<FolderDto>> GetFoldersAsync(MenuType menuType);
        Task<List<FileDto>> GetFilesAsync(MenuType menuType);
        Task<int> CreateEquipmentTableAsync();
        Task<int> CreateSummaryAsync(SummaryFormat summaryFormat);
        Task<int> CreateSummaryFileAsync(string name, int folderId, int summaryId, MenuType menuType);
        Task<int> CreateFolderAsync(string name, int? parentId, MenuType menuType);
        Task<int> CreateFileAsync(string name, FileFormat fileFormat, int folderId, int tableId, MenuType menuType);
        Task RenameFolderAsync(int folderId, string newName);
        Task RenameFileAsync(int fileId, string newName);
    }
}
