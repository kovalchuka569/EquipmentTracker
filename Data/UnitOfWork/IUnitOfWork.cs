using Data.Repositories.Interfaces;
using Data.Interfaces;
using Data.Repositories.Interfaces.SummarySheet;

namespace Data.UnitOfWork;

public interface IUnitOfWork
{
    public IFoldersRepository FoldersRepository { get; }
    
    public IFilesRepository FilesRepository { get; }

    public IEquipmentSheetRepository EquipmentSheetRepository { get; }
    
    public ISummarySheetsRepository SummarySheetRepository { get; }
    

    Task EnsureInitializedForReadAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task<int> CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    ValueTask DisposeAsync();
}
