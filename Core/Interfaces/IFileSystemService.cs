using Common.Enums;
using Models.FileSystem;

namespace Core.Interfaces;

public interface IFileSystemService
{
    /// <summary>
    /// Gets the childs file system items by parent ID.
    /// </summary>
    /// <param name="menuType">The type of menu from which the items are requested.</param>
    /// <param name="parentId">The unique identifier of the parent item. Null for root items.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of file system items.</returns>
    public Task<List<FileSystemItemModel>> GetChildsAsync (MenuType menuType, Guid? parentId, CancellationToken ct = default);
    
    /// <summary>
    /// Inserts a new child file system item.
    /// </summary>
    /// <param name="fileSystemItemModel">The model containing the data for the new file system item.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task InsertChildAsync (FileSystemItemModel fileSystemItemModel, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes the specified file system item asynchronously.
    /// </summary>
    /// <param name="fileSystemId">ID of file system item.</param>
    /// <param name="isMarkedForDelete">New has marked for delete flag value for file.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task UpdateHasMarkedForDelete (Guid fileSystemId, bool isMarkedForDelete, CancellationToken ct = default);
    
    /// <summary>
    /// Renames the specified file system item asynchronously.
    /// </summary>
    /// <param name="renamedFileId">The unique identifier of the file system item to rename.</param>
    /// <param name="newName">The new name for the file system item.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task RenameFileSystemItemAsync (Guid renamedFileId, string newName, CancellationToken ct = default);
    
    /// <summary>
    /// Updates the HasChilds flag for the specified file system item.
    /// </summary>
    /// <param name="fileSystemItemId">The unique identifier of the file system item to update.</param>
    /// <param name="hasChilds">Indicates whether the item has children.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task UpdateHasChildsAsync(Guid fileSystemItemId, bool hasChilds, CancellationToken ct = default);

    /// <summary>
    /// Updates the HasChilds flag for multiple file system items.
    /// </summary>
    /// <param name="newStatuses">
    /// A list of tuples containing new HasChilds data for each file system element:
    /// <list type="bullet">
    /// <item><description>Id - The unique identifier of the file system item to update.</description></item>
    /// <item><description>HasChilds - The new status.</description></item>
    /// </list>
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    public Task UpdateHasChildsAsync(List<(Guid Id, bool HasChilds)> newStatuses, CancellationToken ct = default);
    
    /// <summary>
    /// Updates the parent relationships and order positions for multiple file system items asynchronously.
    /// </summary>
    /// <param name="newPositions">
    /// A list of tuples containing the new position data for each file system item:
    /// <list type="bullet">
    /// <item><description>Id - The unique identifier of the file system item to update.</description></item>
    /// <item><description>NewParentId - The new parent item ID (null for root level items).</description></item>
    /// <item><description>NewOrder - The new order position within the parent.</description></item>
    /// </list>
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    public Task UpdateParentsAndOrdersAsync(List<(Guid Id, Guid? NewParentId, int NewOrder)> newPositions, CancellationToken ct = default);
}