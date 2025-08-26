using System;

using Common.Enums;


namespace Presentation.ViewModels.Common.FileSystem;

public class EquipmentSheetFileViewModel : FileSystemItemBaseViewModel
{
    private Guid? _equipmentSheetId;

    public Guid? EquipmentSheetId
    {
        get => _equipmentSheetId;
        set => SetProperty(ref _equipmentSheetId, value);
    }
    
    public EquipmentSheetFileViewModel()
    {
        Format = FileFormat.EquipmentSheet;
    }
    
}