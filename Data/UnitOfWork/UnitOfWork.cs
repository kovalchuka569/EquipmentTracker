using Data.ApplicationDbContext;
using Data.Repositories;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitOfWork;

public class UnitOfWork(IDbContextFactory<AppDbContext> contextFactory) : IUnitOfWork, IDisposable, IAsyncDisposable
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private AppDbContext? _activeContext;
    private bool _isContextInitialized;
    private bool _disposed;
    
    // Lazy repositories
    private IFileSystemRepository? _fileSystemRepository;
    private IEquipmentSheetRepository? _equipmentSheetRepository;
    private IPivotSheetRepository? _pivotSheetRepository;

    private async Task InitializeContextAsync(CancellationToken ct = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));
            
        if (_isContextInitialized) 
            return;

        await _initializationSemaphore.WaitAsync(ct);
        try
        {
            if (!_isContextInitialized)
            {
                _activeContext = await _contextFactory.CreateDbContextAsync(ct);
                _isContextInitialized = true;
            }
        }
        finally
        {
            _initializationSemaphore.Release();
        }
    }
    
    // Repository properties with null-check
    
    public IFileSystemRepository FileSystemRepository
    {
        get
        {
            ThrowIfDisposed();
            EnsureContextInitialized();
            return _fileSystemRepository ??= new FileSystemRepository(_activeContext!);
        }
    }

    public IEquipmentSheetRepository EquipmentSheetRepository
    {
        get
        {
            ThrowIfDisposed();
            EnsureContextInitialized();
            return _equipmentSheetRepository ??= new EquipmentSheetRepository(_activeContext!);
        }
    }


    public IPivotSheetRepository PivotSheetRepository
    {
        get
        {
            ThrowIfDisposed();
            EnsureContextInitialized();
            return _pivotSheetRepository ??= new PivotSheetRepository(_activeContext!);
        }
    }
    
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        ThrowIfDisposed();
        await InitializeContextAsync(ct);
        await _activeContext!.BeginTransactionAsync(ct);
    }
    
    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        ThrowIfDisposed();
        if (_activeContext == null)
            throw new InvalidOperationException("Context is not initialized. Call BeginTransactionAsync first.");
            
        try
        {
            var result = await _activeContext.SaveChangesAsync(ct);
            await _activeContext.CommitTransactionAsync(ct);
            return result;
        }
        catch
        {
            await _activeContext.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        ThrowIfDisposed();
        if (_activeContext != null)
        {
            await _activeContext.RollbackTransactionAsync(ct);
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct)
    {
        try
        {
            await BeginTransactionAsync(ct);
            await action();
            await CommitAsync(ct);
        }
        catch (Exception)
        {
            await RollbackAsync(ct);
            throw;
        }
    }

    public async Task EnsureInitializedForReadAsync(CancellationToken ct = default)
    {
        await InitializeContextAsync(ct);
    }

    private void EnsureContextInitialized()
    {
        if (!_isContextInitialized)
            throw new InvalidOperationException("Context is not initialized. Call EnsureInitializedForReadAsync or BeginTransactionAsync first.");
    }
    
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _activeContext?.Dispose();
        _initializationSemaphore.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) 
        {
            return;
        }

        if (_activeContext != null)
        {
            await _activeContext.DisposeAsync();
        }

        _initializationSemaphore.Dispose();

        _disposed = true;
        
    }
}