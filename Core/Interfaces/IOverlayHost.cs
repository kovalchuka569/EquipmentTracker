namespace Core.Interfaces;

public interface IOverlayHost
{
    bool IsOverlayOpen { get; set; }

    object? OverlayContent { get; set; }
}