using System.Windows;

namespace EquipmentTracker.Factories.Implementations.Syncfusion;

public interface IGridInteractionHandler
{
    /// <summary>
    /// Handler for hyperlink cell click event.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Route data.</param>
    void OnHyperlinkCellClick(object sender, RoutedEventArgs e);
}