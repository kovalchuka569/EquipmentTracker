using Common.Enums;


namespace Presentation.ViewModels.Common.FileSystem;

public class FolderViewModel : FileSystemItemBaseViewModel
{
    public FolderViewModel()
    {
        Format = FileFormat.Folder;
    }
    
}