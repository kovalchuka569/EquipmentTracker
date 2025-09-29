using System;
using System.Threading.Tasks;
using Presentation.Contracts;
using Syncfusion.Windows.Controls.Notification;

namespace Presentation.Interfaces;

public interface IBusyIndicatorManager
{
    private const string DefaultBusyIndicatorColor = "#ffffff";

    void ShowBusyIndicator(IBusyIndicatorHost busyIndicatorHost, string? header = null,
        string busyColor = DefaultBusyIndicatorColor, AnimationTypes animationType = AnimationTypes.DotCircle);

    void HideBusyIndicator(IBusyIndicatorHost busyIndicatorHost);

    Task<T> ExecuteWithBusyIndicatorAsync<T>(Func<Task<T>> action, IBusyIndicatorHost busyIndicatorHost,
        string? header = null, string busyColor = DefaultBusyIndicatorColor,
        AnimationTypes animationType = AnimationTypes.DotCircle);

    Task<T> ExecuteWithBusyIndicatorAndOverlayAsync<T>(Func<Task<T>> action,
        IOverlayManager overlayManager, IOverlayHost overlayHost, IBusyIndicatorHost busyIndicatorHost,
        string? header = null,
        string busyColor = DefaultBusyIndicatorColor, AnimationTypes animationType = AnimationTypes.DotCircle);
}