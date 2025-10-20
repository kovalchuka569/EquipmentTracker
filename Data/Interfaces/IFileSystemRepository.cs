using Data.Entities.MainTree;
using Common.Enums;

namespace Data.Interfaces;

public interface IFileSystemRepository
{
    /// <summary>
    /// Gets the list of childs file system item entities by menu type and parent ID.
    /// </summary>
    /// <param name="menuType">Menu type.</param>
    /// <param name="parentId">Parent ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of file system item entities.</returns>
    public Task<List<MainTreeItemEntity>> GetChildsFileSystemItemsByMenuTypeAsync(MenuType menuType, Guid? parentId, CancellationToken ct = default);
    
    /// <summary>
    /// Add a new file system item entity
    /// </summary>
    /// <param name="item">New file system item entity.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task AddFileSystemItemAsync(MainTreeItemEntity item, CancellationToken ct = default);
    
    /// <summary>
    /// Rename file system item by ID.
    /// </summary>
    /// <param name="fileSystemItemId"> File system item ID.</param>
    /// <param name="newName">New name</param>
    /// <param name="ct">Cancellation token.</param>
    public Task RenameFileSystemItemAsync(Guid fileSystemItemId, string newName, CancellationToken ct = default);
    
    /// <summary>
    /// Updates the HasChilds flag for the specified file system item in the database.
    /// </summary>
    /// <param name="fileSystemItemId">The ID of the file system item to update.</param>
    /// <param name="hasChilds">Indicates whether the item has children.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task UpdateHasChildsAsync(Guid fileSystemItemId, bool hasChilds, CancellationToken ct = default);
    
    /// <summary>
    /// Updates the HasChilds flag for the specified file system item.
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
    /// <item><description>Id - The unique identifier of the file system item to update</description></item>
    /// <item><description>NewParentId - The new parent item ID (null for root level items)</description></item>
    /// <item><description>NewOrder - The new order position within the parent</description></item>
    /// </list>
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    public Task UpdateParentsAndOrdersAsync(List<(Guid Id, Guid? NewParentId, int NewOrder)> newPositions, CancellationToken ct = default);
    
    /// <summary>
    /// Updates is marked for delete file system item.
    /// </summary>
    /// <param name="fileSystemItemId">File system item ID.</param>
    /// <param name="isMarkedForDelete">Is marked for delete new value.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task UpdateIsMarkedForDeleteAsync(Guid fileSystemItemId, bool isMarkedForDelete, CancellationToken ct = default);
}

