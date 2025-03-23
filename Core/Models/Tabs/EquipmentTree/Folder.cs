namespace Core.Models.Tabs.ProductionEquipmentTree;

public class Folder
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public bool IsExpanded { get; set; }
    private string _imageIcon { get; set; }
    public List<Folder> SubFolders { get; set; }

    public string ImageIcon
    {
        get => _imageIcon;
        set => _imageIcon = value;
    }
    
    
    public Folder()
    {
        SubFolders = new List<Folder>();
        ImageIcon = "Assets/folder.png";
        IsExpanded = false;
    }

    public void UpdateIcon()
    {
        ImageIcon = IsExpanded ? "Assets/folder.png" : "Assets/opened_folder.png";
    }
}