namespace Models.EquipmentTree
{
    public class FolderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string MenuType { get; set; }
    }
}

