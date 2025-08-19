using Core.Interfaces;

namespace Presentation.Interfaces;

public interface IOverlayManager
{
    void ShowOverlay(IOverlayHost overlayHost, string overlayColor = "#000000", double overlayOpacity = 0.5);

    void HideOverlay(IOverlayHost overlayHost);
}