using Data.Repositories.EquipmentSheet;
using Data.Repositories.Interfaces;
using Data.Repositories.Interfaces.EquipmentSheet;
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
        containerRegistry.RegisterScoped<IColumnRepository, ColumnRepository>();
        containerRegistry.RegisterScoped<IRowsRepository, RowsRepository>();
        containerRegistry.RegisterScoped<ICellsRepository, CellsRepository>();
        containerRegistry.RegisterScoped<ISummarySheetsRepository, SummarySheetsRepository>();
        containerRegistry.RegisterScoped<ISummarySheetColumnsRepository, SummarySheetColumnsRepository>();
        containerRegistry.RegisterScoped<ISummarySheetRowsRepository, SummarySheetRowsRepository>();
        containerRegistry.RegisterScoped<IEquipmentSheetRepository, EquipmentSheetRepository>();
        containerRegistry.RegisterScoped<IEquipmentColumnsRepository, EquipmentColumnsRepository>();
        containerRegistry.RegisterScoped<IEquipmentRowsRepository, EquipmentRowsRepository>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}