using System;

using Common.Enums;

using Models.FileSystem;

using Presentation.ViewModels.Common.FileSystem;

namespace Presentation.Mappers;

public static class FileSystemMapper
{
    #region Constants
    
    #region Error messages
    
    private const string FileFormatNotSupportedException = "File format is not supported.";
    
    private const string UnknownViewModelTypeException = "Unknow view model typу.";
    
    private const string UnknownModelTypeException = "Unknown model type.";
    
    #endregion

    #region Default file names
    
    private const string NewUndefinedName = "Новий невіодмий файл";

    private const string DefaultNewFolderName = "Нова папка";
    
    private const string DefaultNewEquipmentSheetName = "Новий лист обладнання";

    private const string DefaultNewPivotSheetName = "Новий звідний лист";

    #endregion
    
    #region View names

    private const string EquipmentSheetViewName = "EquipmentSheetView";
    
    private const string PivotSheetViewName = "PivotSheetView";
    
    #endregion
    
    #endregion

    public static FileSystemItemModel ToDomain(FileSystemItemBaseViewModel viewModel)
    {
        FileSystemItemModel model = viewModel switch
        {
            EquipmentSheetFileViewModel ef => new EquipmentSheetFileModel
            {
                EquipmentSheetId = ef.EquipmentSheetId
            },
            PivotSheetFileViewModel pf => new PivotSheetFileModel
            {
                PivotSheetId = pf.PivotSheetId
            },
            FolderViewModel => new FolderModel(),
            _ => throw new NotSupportedException(UnknownViewModelTypeException)
        };
        
        model.Id = viewModel.Id;
        model.Name = viewModel.Name;
        model.Order = viewModel.Order;
        model.HasChilds = viewModel.HasChilds;
        model.ParentId = viewModel.ParentId;
        model.Format = viewModel.Format;
        model.MenuType = viewModel.MenuType;
        model.Deleted = viewModel.Deleted;
        
        return model;
    }

    public static FileSystemItemBaseViewModel ToViewModel(FileSystemItemModel domain)
    {
        FileSystemItemBaseViewModel viewModel = domain switch
        {
            EquipmentSheetFileModel ef => new EquipmentSheetFileViewModel
            {
                EquipmentSheetId = ef.EquipmentSheetId
            },
            PivotSheetFileModel pf => new PivotSheetFileViewModel
            {
                PivotSheetId = pf.PivotSheetId
            },
            FolderModel => new FolderViewModel(),
            _ => throw new NotSupportedException(UnknownModelTypeException)
        };
        
        viewModel.Id = domain.Id;
        viewModel.Name = domain.Name;
        viewModel.Order = domain.Order;
        viewModel.HasChilds = domain.HasChilds;
        viewModel.ParentId = domain.ParentId;
        viewModel.Format = domain.Format;
        viewModel.MenuType = domain.MenuType;
        viewModel.Deleted = domain.Deleted;

        if (viewModel.HasChilds)
        {
            viewModel.Childs.Add(new DummyFileViewModel());
        }

        return viewModel;
    }
    
    public static FileSystemItemBaseViewModel FileFormatToViewModel(FileFormat fileFormat)
    {
        FileSystemItemBaseViewModel newItem = fileFormat switch
        {
            FileFormat.Folder => new FolderViewModel(),
            FileFormat.EquipmentSheet => new EquipmentSheetFileViewModel(),
            FileFormat.PivotSheet => new PivotSheetFileViewModel(),
            _ => throw new NotSupportedException(FileFormatNotSupportedException)
        };

        return newItem;
    }

    public static string FileFormatToNewName(FileFormat fileFormat)
    {
        switch (fileFormat)
        {
            case FileFormat.Folder:
                return DefaultNewFolderName;
            case FileFormat.EquipmentSheet:
                return DefaultNewEquipmentSheetName;
            case FileFormat.PivotSheet:
                return DefaultNewPivotSheetName;

            case FileFormat.None:
            case FileFormat.RepairsSheet:
            case FileFormat.ServicesSheet:
            case FileFormat.WriteOffSheet:
            default:
                return NewUndefinedName;
        }
    }

    public static string FileFormatToViewName(FileFormat fileFormat)
    {
        switch (fileFormat)
        {
            case FileFormat.EquipmentSheet:
                return EquipmentSheetViewName;
            case FileFormat.PivotSheet:
                return PivotSheetViewName;
            case FileFormat.None:
            case FileFormat.Folder:
            case FileFormat.RepairsSheet:
            case FileFormat.ServicesSheet:
            case FileFormat.WriteOffSheet:
            default:
                return string.Empty;
        }
    }
}