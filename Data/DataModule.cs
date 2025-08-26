using Data.Interfaces;
using Data.Repositories;
using Data.UnitOfWork;

namespace Data;

public class DataModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        
        containerRegistry.RegisterScoped<IPivotSheetRepository, PivotSheetRepository>();
        
        containerRegistry.RegisterScoped<IEquipmentSheetRepository, EquipmentSheetRepository>();
        
        containerRegistry.RegisterScoped<IFileSystemRepository, FileSystemRepository>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}