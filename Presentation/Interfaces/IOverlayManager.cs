using System;
using System.Threading.Tasks;
using Presentation.Contracts;

namespace Presentation.Interfaces;

public interface IOverlayManager
{
    void ShowOverlay(IOverlayHost overlayHost, string overlayColor = "#000000", double overlayOpacity = 0.5);

    void HideOverlay(IOverlayHost overlayHost);
    
    /// <summary>
    /// Executes the specified asynchronous action while displaying an overlay
    /// on the given <paramref name="overlayHost"/>.  
    /// The overlay is shown before the action starts and hidden after it completes,
    /// even if an exception occurs.
    /// </summary>
    /// <param name="action">
    /// The asynchronous operation to run while the overlay is visible.
    /// </param>
    /// <param name="overlayHost">
    /// The host control or view where the overlay should be displayed.
    /// </param>
    /// <param name="overlayColor">
    /// Optional hex color for the overlay background (default is "#000000").
    /// </param>
    /// <param name="overlayOpacity">
    /// Optional opacity value for the overlay background (default is 0.5).
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous execution of the action.
    /// </returns>
    Task<T> ExecuteWithOverlayAsync<T>(Func<Task<T>> action, IOverlayHost overlayHost, string overlayColor = "#000000", double overlayOpacity = 0.5);
}