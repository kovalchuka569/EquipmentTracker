namespace Models.RepairsDataGrid;

public class RepairItem
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string BreakDescription { get; set; }
    public string BreakDescriptionDisplay => BreakDescription;
    public string EquipmentInventoryNumber { get; set; }
    public string EquipmentInventoryNumberDisplay => EquipmentInventoryNumber;
    public string EquipmentBrand { get; set; }
    public string EquipmentBrandDisplay => EquipmentBrand;
    public string EquipmentModel { get; set; }
    public string EquipmentModelDisplay => EquipmentModel;
    public DateTime? StartDate { get; set; }
    public string StartDateDisplay => StartDate.HasValue ? StartDate.Value.ToString("dd.MM.yyyy HH:mm") : "";
    public DateTime? EndDate { get; set; } 
    public string EndDateDisplay => EndDate.HasValue ? EndDate.Value.ToString("dd.MM.yyyy HH:mm") : "";
    
    public TimeSpan? Duration { get; set; }
    public string DurationDisplay
    {
        get
        {
            if (!Duration.HasValue) return "";
            if (Duration.Value == TimeSpan.Zero) return "";
            
            var parts = new List<string>();
            if (Duration.Value.Days > 0)
                parts.Add($"{Duration.Value.Days} днів");
            if (Duration.Value.Hours > 0)
                parts.Add($"{Duration.Value.Hours} годин");
            if (Duration.Value.Minutes > 0)
                parts.Add($"{Duration.Value.Minutes} хвилин");

            return parts.Count > 0 ? string.Join(", ", parts) : "0 хвилин";
        }
    }
    public int Worker { get; set; }
    public string WorkerDisplay => Worker.ToString();
    public string Status {get; set; }
    public string StatusDisplay => Status;
    public string StatusBackground => StatusToBackground(Status);
    
    private string StatusToBackground(string statusName)
    {
        switch (statusName)
        {
            case "Заплановано":
                return "#8080CFFA";
            case "Очікує підтвердження":
                return "#80FFC04D";
            case "Очікує матеріали":
                return "#80FFD700";
            case "В процессі":
                return "#8032CD32";
            case "Призупинений":
                return "#80D3D3D3";
            case "Виконано":
                return "#804CAF50";
            case "Скасовано":
                return "#80F08080";
            case "Діагностика":
                return "#8087CEFA";
            case "Передано підряднику":
                return "#80BA55D3";
            default: return "White";
        }
    }
}