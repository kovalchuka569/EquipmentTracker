using System.ComponentModel;
using Core.Contracts;

namespace EquipmentTracker.Common.OverlayManager;

public interface IOverlayManager
{
    void ShowOverlay(IOverlayHost overlayHost, string overlayColor = "#000000", double overlayOpacity = 0.5);

    void HideOverlay(IOverlayHost overlayHost);
}