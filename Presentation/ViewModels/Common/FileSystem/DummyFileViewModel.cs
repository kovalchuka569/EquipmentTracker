using Common.Enums;

namespace Presentation.ViewModels.Common.FileSystem;

public class DummyFileViewModel : FileSystemItemBaseViewModel
{
    public DummyFileViewModel()
    {
        Format = FileFormat.None;
    }
}