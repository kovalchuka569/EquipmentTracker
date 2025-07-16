using Models.Equipment;

namespace Models.Summary.ColumnTree;

public class FileDto
{
    public int Id { get; set; }
    public int? FolderId { get; set; }
    public int TableId { get; set; }
    public string Name { get; set; }
}