using Models.Enums;

namespace Models.EquipmentTree
{
    public class FolderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public MenuType MenuType { get; set; }
    }
}

