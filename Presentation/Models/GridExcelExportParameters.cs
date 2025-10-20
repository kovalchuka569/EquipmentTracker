using System;
using System.Threading.Tasks;
using Syncfusion.UI.Xaml.Grid.Converter;

namespace Presentation.Models;

public class GridExcelExportParameters
{
    public required ExcelExportingOptions ExcelExportingOptions { get; set; }
    public required string Path { get; set; }
    public required IProgress<int> Progress { get; set; }
    public Action OnSuccess { get; set; } = delegate { };
    public Action<Exception> OnError { get; set; } = delegate { };
    public Action OnFinally { get; set; } = delegate { };
}