using System;
using Syncfusion.Windows.Shared.Printing;

namespace Presentation.Models;

public class GridPrintParameters
{
    public bool AllowColumnWidthFitToPrintPage { get; set; } = true;
    public bool AllowPrintByDrawing { get; set; }
    public bool AllowRepeatHeaders { get; set; }
    public bool CanPrintStackedHeaders { get; set; } = true;
    public PrintOrientation PrintPageOrientation { get; set; } = PrintOrientation.Landscape;
}