using Models.Entities.FileSystem;
using Models.Enums;

namespace Data.Repositories.Interfaces;

public interface IFilesRepository
{
    /// <summary>
    /// Get file entity by id
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of file entities</returns>
    public Task<FileEntity> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Get list files entities by menu type
    /// </summary>
    /// <param name="menuType">Menu type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of file entities</returns>
    public Task<List<FileEntity>> GetListByMenuTypeAsync(MenuType menuType, CancellationToken ct = default);
    
    /// <summary>
    /// Add new file
    /// </summary>
    /// <param name="file">File entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task AddAsync(FileEntity file, CancellationToken ct = default);
    
    /// <summary>
    /// Update file
    /// </summary>
    /// <param name="file">File entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateAsync(FileEntity  file, CancellationToken ct = default);

    /// <summary>
    /// Rename file
    /// </summary>
    /// <param name="id">File id</param>
    /// <param name="newName">File new name</param>
    /// <param name="ct">CancellationToken</param>
    public Task RenameAsync(Guid id, string newName, CancellationToken ct = default);

    /// <summary>
    /// Update delete status of file
    /// </summary>
    /// <param name="id">File id</param>
    /// <param name="deleted">New delete status</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct = default);
}