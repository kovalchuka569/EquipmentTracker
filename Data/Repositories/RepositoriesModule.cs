using Data.Repositories.Interfaces;
using Data.Interfaces;
using Data.Repositories.Interfaces.SummarySheet;
using Data.Repositories.SummarySheet;
using Data.UnitOfWork;

namespace Data.Repositories;

public class RepositoriesModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        containerRegistry.RegisterScoped<IFoldersRepository, FoldersRepository>();
        containerRegistry.RegisterScoped<IFilesRepository, FilesRepository>();
        containerRegistry.RegisterScoped<ISummarySheetsRepository, SummarySheetsRepository>();
        containerRegistry.RegisterScoped<IEquipmentSheetRepository, EquipmentSheetRepository>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}