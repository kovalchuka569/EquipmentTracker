using Data.Repositories.Interfaces;
using Data.Repositories.Interfaces.EquipmentSheet;
using Data.Repositories.Interfaces.SummarySheet;

namespace Data.UnitOfWork;

public interface IUnitOfWork
{
    public IFoldersRepository FoldersRepository { get; }
    public IFilesRepository FilesRepository { get; }

    public IColumnRepository ColumnRepository { get; }
    public IRowsRepository RowsRepository { get; }
    public ICellsRepository CellRepository { get; }

    public IEquipmentSheetRepository EquipmentSheetRepository { get; }
    public IEquipmentColumnsRepository EquipmentColumnsRepository { get; }
    public IEquipmentRowsRepository EquipmentRowsRepository { get; }

    public ISummarySheetsRepository SummarySheetRepository { get; }
    public ISummarySheetRowsRepository SummarySheetRowsRepository { get; }
    public ISummarySheetColumnsRepository SummarySheetColumnsRepository { get; }

    Task EnsureInitializedForReadAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task<int> CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    ValueTask DisposeAsync();
}
