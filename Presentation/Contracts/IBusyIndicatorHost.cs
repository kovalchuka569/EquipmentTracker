namespace Presentation.Contracts;

public interface IBusyIndicatorHost
{
    public bool IsBusy { get; set; }
    public object? BusyContent { get; set; }
}