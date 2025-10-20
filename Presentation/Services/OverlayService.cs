using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using Presentation.Contracts;
using Presentation.Services.Interfaces;
using Presentation.Services.Builders;
using Presentation.Services.Internal;

namespace Presentation.Services;

public class OverlayService : IOverlayService
{
    public OverlayBuilder Configure()
    {
        return new OverlayBuilder(this);
    }
    
    internal async Task<T> ExecuteAsync<T>(OverlayConfiguration config, Func<Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(config.Host);
        ArgumentNullException.ThrowIfNull(action);

        ShowOverlay(config);
        try
        {
            return await action();
        }
        finally
        {
            HideOverlay(config.Host);
        }
    }
    
    private void ShowOverlay(OverlayConfiguration config)
    {
        var overlay = new Rectangle
        {
            Opacity = config.Opacity,
            Fill = (Brush)new BrushConverter().ConvertFromString(config.Color)!
        };

        config.Host.IsOverlayOpen = true;
        config.Host.OverlayContent = overlay;
    }

    private void HideOverlay(IOverlayHost host)
    {
        host.IsOverlayOpen = false;
        host.OverlayContent = null;
    }
}