using Core.Contracts;
using EquipmentTracker.Common.Controls;

namespace EquipmentTracker.Common.OverlayManager;

public class OverlayManager : IOverlayManager
{
    public void ShowOverlay(IOverlayHost overlayHost, string overlayColor, double overlayOpacity)
    {
        var overlay = new Overlay
        {
           OverlayOpacity = overlayOpacity
        };
        
        overlayHost.IsOverlayOpen = true;
        overlayHost.OverlayContent = overlay;
    }

    public void HideOverlay(IOverlayHost overlayHost)
    {
        overlayHost.IsOverlayOpen = false;
        overlayHost.OverlayContent = null;
    }
}