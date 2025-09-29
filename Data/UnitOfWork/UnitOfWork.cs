using Data.ApplicationDbContext;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
{

    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public IFileSystemRepository FileSystemRepository { get; }
    public IEquipmentSheetRepository EquipmentSheetRepository { get; }
    public IPivotSheetRepository PivotSheetRepository { get; }
    public UnitOfWork(AppDbContext context,
        IFileSystemRepository fileSystemRepository,
        IEquipmentSheetRepository equipmentSheetRepository,
        IPivotSheetRepository pivotSheetRepository)
    {
        _context = context;
        
        FileSystemRepository = fileSystemRepository;
        EquipmentSheetRepository = equipmentSheetRepository;
        PivotSheetRepository = pivotSheetRepository;
    }
    
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
       return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if(_transaction is null)
            return;
        
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if(_transaction is null)
            return;
        
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
    
    
    public void Dispose()
    {
        if (_disposed) 
            return;
        
        _context.Dispose();

        if (_transaction is not null)
        {
            _transaction.Dispose();
            _transaction = null;
        }
        
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) 
            return;

        await _context.DisposeAsync();
        
        if(_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        
        _disposed = true;
    }
}