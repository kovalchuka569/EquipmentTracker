
namespace Presentation.ViewModels.Common.PivotGrid;

public class Sale
{
    public string Product { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Amount { get; set; }
}