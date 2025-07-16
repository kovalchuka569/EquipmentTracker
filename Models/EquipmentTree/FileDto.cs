namespace Models.EquipmentTree
{
    public class FileDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FolderId { get; set; }
        public FileFormat FileFormat { get; set; }
        public int? SummaryId { get; set; }
        public int TableId { get; set; }
    }
}

