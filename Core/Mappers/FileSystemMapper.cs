using Models.Entities.FileSystem;
using Models.FileSystem;

namespace Core.Mappers;

public static class FileSystemMapper
{
    public static FileSystemItemEntity FileSystemItemModelToEntity(FileSystemItemModel model)
    {
        var entity = model switch
        {
            FolderModel => new FolderEntity(),
            EquipmentSheetFileModel ef => new EquipmentFileEntity
            {
                EquipmentSheetId = ef.EquipmentSheetId
            },
            PivotSheetFileModel pf => new PivotFileEntity
            {
                PivotSheetId = pf.PivotSheetId
            },
            _ => new FileSystemItemEntity()
        };

        entity.Id = model.Id;
        entity.Name = model.Name;
        entity.Order = model.Order;
        entity.HasChilds = model.HasChilds;
        entity.ParentId = model.ParentId;
        entity.Format = model.Format;
        entity.MenuType = model.MenuType;
        entity.Deleted = model.Deleted;
        
        return entity;
    }

    public static FileSystemItemModel FileSystemItemEntityToModel(FileSystemItemEntity entity)
    {
        var model = entity switch
        {
            FolderEntity => new FolderModel(),
            EquipmentFileEntity ef => new EquipmentSheetFileModel
            {
                EquipmentSheetId = ef.EquipmentSheetId
            },
            PivotFileEntity pf => new PivotSheetFileModel
            {
                PivotSheetId = pf.PivotSheetId
            },
            _ => new FileSystemItemModel()
        };
        
        model.Id = entity.Id;
        model.Name = entity.Name;
        model.Order = entity.Order;
        model.HasChilds = entity.HasChilds;
        model.ParentId = entity.ParentId;
        model.Format = entity.Format;
        model.MenuType = entity.MenuType;
        model.Deleted = entity.Deleted;
        
        return model;
    }
}