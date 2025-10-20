namespace Data.Entities.Base;

public abstract class TreeItemBaseEntity
{
    public Guid Id { get; set; }
    
    public Guid? ParentId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public int Order { get; set; }
}