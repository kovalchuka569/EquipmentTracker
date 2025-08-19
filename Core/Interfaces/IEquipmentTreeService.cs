using System.Collections.ObjectModel;

using Models.EquipmentTree;
using Models.Enums;

namespace Core.Interfaces;
public interface IEquipmentTreeService
{
    
    /// <summary>
    /// Get hierarchical tree of files and folders based on the specified menu type.
    /// </summary>
    /// <param name="menuType">Menu type</param>
    /// <returns>Collection of file and folder items</returns>
    Task<ObservableCollection<IFileSystemItem>> BuildHierarchy(MenuType menuType);
    
    /// <summary>
    /// Create a new folder
    /// </summary>
    /// <param name="folderItem">Folder item</param>
    /// <param name="ct">Cancellation token</param>
    Task CreateFolderAsync(FolderItem folderItem, CancellationToken ct = default);
    
    /// <summary>
    /// Create a new file
    /// </summary>
    /// <param name="fileItem">File item</param>
    /// <param name="fileFormat">File format</param>
    /// <param name="ct">Cancellation token</param>
    Task CreateFileAsync(FileItem fileItem, FileFormat fileFormat, CancellationToken ct = default);
    
    /// <summary>
    /// Generate unique name based on incoming name and existing names
    /// <example>
    /// "NewFolder" -> "NewFolder (1)" if "NewFolder" already exists in existing names
    /// </example>
    /// </summary>
    /// <param name="incomingName">Incoming name</param>
    /// <param name="existingNames">List of existing names</param>
    /// <returns>Unique name</returns>
    string GenerateUniqueFileFolderName (string incomingName, List<string> existingNames);
    
    /// <summary>
    /// Rename folder
    /// </summary>
    /// <param name="folderId">Folder id</param>
    /// <param name="newName">New name</param>
    /// <param name="ct">Cancellation token</param>
    Task RenameFolderAsync(Guid folderId, string newName, CancellationToken ct = default);
    
    /// <summary>
    /// Rename file
    /// </summary>
    /// <param name="fileId">File id</param>
    /// <param name="newName">New name</param>
    /// <param name="ct">Cancellation token</param>
    Task RenameFileAsync(Guid fileId, string newName, CancellationToken ct = default);
}
