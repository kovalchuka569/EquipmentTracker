namespace Models.RepairsDataGrid.AddService;

public class ServiceTypeItem
{
    public string TypeName { get; set; }
    public string TypeBackground => ServiceTypeToBackground(TypeName);
    
    private string ServiceTypeToBackground(string serviceType)
    {
        switch (serviceType)
        {
            case "Щоденне обслуговування":
                return "#80E0FFFF"; 
            case "Періодичне технічне обслуговування":
                return "#8090EE90"; 
            case "Сезонне обслуговування":
                return "#80FFE4B5"; 
            case "Калібрування / Перевірка":
                return "#80FFD700";
            case "Діагностика":
                return "#8087CEFA"; 
            case "Модернізація / Поліпшення":
                return "#80D8BFD8";
            case "Інше":
                return "#80D3D3D3"; 
            default:
                return "White";
        }
    }
}