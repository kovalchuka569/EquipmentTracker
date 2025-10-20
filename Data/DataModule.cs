using Data.ApplicationDbContext;
using Data.Interfaces;
using Data.Repositories;
using Data.UnitOfWork;
using LocalSecure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class DataModule : IModule
{
    
    private readonly IDbKeyService _dbKeyService;

    public DataModule(IDbKeyService dbKeyService)
    {
        _dbKeyService = dbKeyService;
    }
    
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterScoped<AppDbContext>(_ =>
        {
            var connectionString = _dbKeyService.LoadDbConnectionString();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        });
        
        containerRegistry.RegisterScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        containerRegistry.RegisterScoped<IUserRepository, UserRepository>();
        containerRegistry.RegisterScoped<IPivotSheetRepository, PivotSheetRepository>();
        containerRegistry.RegisterScoped<IEquipmentSheetRepository, EquipmentSheetRepository>();
        containerRegistry.RegisterScoped<IFileSystemRepository, FileSystemRepository>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}