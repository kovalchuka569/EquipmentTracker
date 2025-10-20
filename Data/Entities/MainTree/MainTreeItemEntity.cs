using Common.Enums;
using Data.Entities.Base;

namespace Data.Entities.MainTree;

public class MainTreeItemEntity : TreeItemBaseEntity
{
    public bool HasChilds { get; set; }
    
    public FileFormat Format { get; set; }
    
    public MenuType MenuType { get; set; }
    
    public bool IsMarkedForDelete { get; set; }
}