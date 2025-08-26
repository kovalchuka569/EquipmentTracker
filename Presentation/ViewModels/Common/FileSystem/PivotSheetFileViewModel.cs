using System;

using Common.Enums;


namespace Presentation.ViewModels.Common.FileSystem;

public class PivotSheetFileViewModel : FileSystemItemBaseViewModel
{
    private Guid? _pivotSheetId;

    public Guid? PivotSheetId
    {
        get => _pivotSheetId;
        set => SetProperty(ref _pivotSheetId, value);
    }
    
    public PivotSheetFileViewModel()
    {
        Format = FileFormat.PivotSheet;
    }
}