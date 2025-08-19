using Core.Interfaces;
using Core.Services;

namespace Core;

public class ServiceModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        
        containerRegistry.RegisterScoped<IEquipmentSheetService, EquipmentSheetService>();
        
        containerRegistry.RegisterScoped<IEquipmentTreeService, EquipmentTreeService>();
        
        containerRegistry.RegisterScoped<IExcelImportService, ExcelImportService>();

        containerRegistry.RegisterScoped<ICellValidatorService, CellValidatorService>();

        containerRegistry.RegisterScoped<IRowValidatorService, RowValidatorService>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}