namespace Core.Models.Tabs.ProductionEquipmentTree;

public class File
{
    public string FileName { get; set; }
    public string ImageIcon { get; set; }
    public List<File> SubFiles { get; set; } = new();
}