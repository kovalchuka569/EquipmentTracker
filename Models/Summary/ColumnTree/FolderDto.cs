namespace Models.Summary.ColumnTree;

public class FolderDto
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; }
}