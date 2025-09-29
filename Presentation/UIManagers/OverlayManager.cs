using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using Presentation.Contracts;
using Presentation.Interfaces;

namespace Presentation.UIManagers;

public class OverlayManager : IOverlayManager
{
    public void ShowOverlay(IOverlayHost overlayHost, string overlayColor, double overlayOpacity)
    {
        var overlay = new Rectangle
        {
           Opacity = overlayOpacity,
           Fill = (Brush)new BrushConverter().ConvertFromString(overlayColor)!,
        };
        
        overlayHost.IsOverlayOpen = true;
        overlayHost.OverlayContent = overlay;
    }

    public void HideOverlay(IOverlayHost overlayHost)
    {
        overlayHost.IsOverlayOpen = false;
        overlayHost.OverlayContent = null;
    }

    public async Task<T> ExecuteWithOverlayAsync<T>(Func<Task<T>> action, IOverlayHost overlayHost,
        string overlayColor = "#000000",
        double overlayOpacity = 0.5)
    {
        ShowOverlay(overlayHost, overlayColor, overlayOpacity);
        try
        {
           return await action();
        }
        finally
        {
            HideOverlay(overlayHost);
        }
    }
}