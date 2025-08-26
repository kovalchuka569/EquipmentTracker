using Core.Interfaces;
using Core.Services;

namespace Core;

public class CoreModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        
        containerRegistry.RegisterScoped<IEquipmentSheetService, EquipmentSheetService>();
        
        containerRegistry.RegisterScoped<IExcelImportService, ExcelImportService>();

        containerRegistry.RegisterScoped<ICellValidatorService, CellValidatorService>();

        containerRegistry.RegisterScoped<IRowValidatorService, RowValidatorService>();
        
        containerRegistry.RegisterScoped<IFileSystemService, FileSystemService>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}