using System.ComponentModel.DataAnnotations.Schema;
using Models.Enums;

namespace Models.Entities.FileSystem;

[Table("Files", Schema = "main")]
public class FileEntity
{
    public Guid Id { get; set; }
    
    public Guid? FolderId { get; set; }
    
    public string Name { get; set; }
    
    public FileFormat FileFormat { get; set; }
    
    public MenuType MenuType { get; set; }
    
    public bool Deleted { get; set; }

    public FolderEntity? Folder { get; set; }
}