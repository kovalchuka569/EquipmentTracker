using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Enums;

namespace Models.Entities.FileSystem;

[Table("Folders", Schema = "main")]
public class FolderEntity
{
    public Guid Id { get; set; }
    
    public Guid? FolderId { get; set; }
    
    public string Name { get; set; }
    
    public MenuType MenuType { get; set; }
    
    public bool Deleted { get; set; }
    
    public FolderEntity? Folder { get; set; }
    
    public List<FolderEntity> Folders { get; set; } = new();
    
    public List<FileEntity> Files { get; set; } = new();
}