using System.Windows.Controls;

namespace Presentation.Interfaces;

public interface ISyncfusionGridPrintManager
{
    ContentControl GetPrintHeaderCell(string mappingName);
    ContentControl GetPrintGridCell(object record, string mappingName);
}