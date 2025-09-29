using Common.Enums;

namespace Models.FileSystem;

public class FileSystemItemModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid? ParentId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public int Order { get; set; }
    
    public bool HasChilds { get; set; }
    
    public FileFormat Format { get; set; } = FileFormat.None;
    
    public MenuType MenuType { get; set; } = MenuType.None;
    
    public bool IsMarkedForDelete { get; set; }
}