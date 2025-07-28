using Models.Entities.FileSystem;
using Models.Enums;

namespace Data.Repositories.Interfaces;

public interface IFoldersRepository
{
    /// <summary>
    /// Get folder entity by id
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of column entities</returns>
    public Task<FolderEntity> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Get list folder entities by menu type
    /// </summary>
    /// <param name="menuType">Menu type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of folders entity</returns>
    public Task<List<FolderEntity>> GetListByMenuTypeAsync(MenuType menuType, CancellationToken ct = default);
    
    /// <summary>
    /// Add new folder
    /// </summary>
    /// <param name="folder">Folder entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task AddAsync(FolderEntity folder, CancellationToken ct = default);
    
    /// <summary>
    /// Update folder
    /// </summary>
    /// <param name="folder">Folder entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateAsync(FolderEntity  folder, CancellationToken ct = default);

    /// <summary>
    /// Rename folder
    /// </summary>
    /// <param name="id">Folder id</param>
    /// <param name="newName">Folder new name</param>
    /// <param name="ct">CancellationToken</param>
    public Task RenameAsync(Guid id, string newName, CancellationToken ct = default);

    /// <summary>
    /// Update delete status of folder
    /// </summary>
    /// <param name="id">Folder id</param>
    /// <param name="deleted">New delete status</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct = default);
}