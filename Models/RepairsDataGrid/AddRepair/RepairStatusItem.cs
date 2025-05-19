namespace Models.RepairsDataGrid.AddRepair;

public class RepairStatusItem
{
    public string StatusName { get; set; }
    public string StatusBackground => StatusToBackground(StatusName);

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