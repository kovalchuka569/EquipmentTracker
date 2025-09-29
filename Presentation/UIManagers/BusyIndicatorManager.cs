using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Presentation.Contracts;
using Presentation.Interfaces;
using Syncfusion.Windows.Controls.Notification;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace Presentation.UIManagers;

public class BusyIndicatorManager : IBusyIndicatorManager
{
    public void ShowBusyIndicator(IBusyIndicatorHost busyIndicatorHost, string? header, string busyColor,
        AnimationTypes animationType)
    {
        var busyIndicator = new SfBusyIndicator
        {
            IsBusy = true,
            AnimationType = animationType,
            Header = header,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = (Brush)new BrushConverter().ConvertFromString(busyColor)!
        };

        busyIndicatorHost.IsBusy = true;
        busyIndicatorHost.BusyContent = busyIndicator;
    }

    public void HideBusyIndicator(IBusyIndicatorHost busyIndicatorHost)
    {
        busyIndicatorHost.IsBusy = false;
        busyIndicatorHost.BusyContent = null;
    }

    public async Task<T> ExecuteWithBusyIndicatorAsync<T>(Func<Task<T>> action, IBusyIndicatorHost busyIndicatorHost,
        string? header,
        string busyColor, AnimationTypes animationType)
    {
        ShowBusyIndicator(busyIndicatorHost, header, busyColor, animationType);
        try
        {
            return await action();
        }
        finally
        {
            HideBusyIndicator(busyIndicatorHost);
        }
    }

    public async Task<T> ExecuteWithBusyIndicatorAndOverlayAsync<T>(Func<Task<T>> action,
        IOverlayManager overlayManager, IOverlayHost overlayHost, IBusyIndicatorHost busyIndicatorHost,
        string? header,
        string busyColor, AnimationTypes animationType)
    {
        return await overlayManager.ExecuteWithOverlayAsync(
            async () => await ExecuteWithBusyIndicatorAsync(action, busyIndicatorHost, header, busyColor,
                animationType),
            overlayHost
        );
    }
}