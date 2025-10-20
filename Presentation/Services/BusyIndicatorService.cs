using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Presentation.Contracts;
using Presentation.Services.Builders;
using Presentation.Services.Interfaces;
using Presentation.Services.Internal;
using Syncfusion.Windows.Controls.Notification;
using Unity;

namespace Presentation.Services;

public class BusyIndicatorService : IBusyIndicatorService
{
    [Dependency] public required IOverlayService OverlayService { get; init; } = null!;

    public BusyIndicatorBuilder ShowBusyIndicator() => new(this);

    internal async Task ResolveBusyIndicatorAsync(BusyIndicatorConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config.Host);
        
        if (config is { WithOverlay: true, Host: IOverlayHost overlayHost })
            await ShowBusyIndicatorWithOverlayAsync(config, overlayHost);

        await ShowBusyIndicatorInternalAsync(config);
    }
    
    internal async Task<T> ExecuteAsync<T>(BusyIndicatorConfiguration config, Func<Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(config.Host);
        ArgumentNullException.ThrowIfNull(action);
        
        if (config is { WithOverlay: true, Host: IOverlayHost overlayHost })
        {
            var overlayBuilder = OverlayService
                .Configure()
                .InHost(overlayHost);
        
            if (config.OverlayBuilder != null)
            {
                overlayBuilder = config.OverlayBuilder(overlayBuilder);
            }
            
            return await overlayBuilder.ExecuteAsync(async () =>
            {
                ShowBusyIndicator(config);
                try
                {
                    return await action();
                }
                finally
                {
                    HideBusyIndicator(config.Host);
                }
            });
        }

        ShowBusyIndicator(config);
        try
        {
            return await action();
        }
        finally
        {
            HideBusyIndicator(config.Host);
        }
    }

    private async Task ShowBusyIndicatorWithOverlayAsync(
        BusyIndicatorConfiguration config,
        IOverlayHost overlayHost)
    {
        var overlayBuilder = OverlayService
            .Configure()
            .InHost(overlayHost);
        
        if (config.OverlayBuilder != null)
        {
            overlayBuilder = config.OverlayBuilder(overlayBuilder);
        }

        await overlayBuilder.ExecuteAsync(
            async () => await ShowBusyIndicatorInternalAsync(config)
        );
    }

    private async Task ShowBusyIndicatorInternalAsync(
        BusyIndicatorConfiguration config)
    {
        var tcs = new TaskCompletionSource();
        try
        {
            ShowBusyIndicator(config);
            await tcs.Task;
        }
        catch
        {
            HideBusyIndicator(config.Host);
            throw;
        }
    }

    private void ShowBusyIndicator(BusyIndicatorConfiguration config)
    {
        var busyIndicator = new SfBusyIndicator
        {
            Header = config.Header,
            AnimationType = config.AnimationType,
            Foreground = (Brush)new BrushConverter().ConvertFromString(config.Color)!
        };

        config.Host.BusyContent = busyIndicator;
        config.Host.IsBusy = true;
    }

    private void HideBusyIndicator(IBusyIndicatorHost? host)
    {
        if (host is null) return;
        
        host.IsBusy = false;
        host.BusyContent = null;
    }
}