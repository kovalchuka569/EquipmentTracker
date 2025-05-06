using System.Collections.ObjectModel;
using Core.Models.Consumables;

namespace Core.Services.Consumables
{
    public interface IConsumablesTreeService
    {
       Task <List<Folder>> GetFoldersAsync();
       Task <List<File>> GetFilesAsync();
       ObservableCollection<IFileSystemItem> BuildHierachy(List<Folder> allFolders, List<File> allFiles);
       Task<int> InsertFolderAsync (Folder folder);
       Task<int> InsertFileAsync(File file);
       Task<string> GenerateUniqueFolderNameAsync (string baseFolderName, int? folderId);
       Task<string> GenerateUniqueFileNameAsync (string baseFileName, int parentFolderId);
       Task RenameFolderAsync(string newName, int folderId);
       Task RenameFileAsync(string newName, string oldName, int fileId);
       Task <Dictionary<string, int>> GetLowValueCountsAsync (List<string> tableNames);
    }
}
