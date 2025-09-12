using System.Collections.Generic;
using Models.Common.Table.ColumnProperties;

namespace Presentation.Models;

public class ColumnEditResult
{
    public List<BaseColumnProperties> NewColumns { get; set; } = [];
    public List<BaseColumnProperties> EditedColumns { get; set; } = [];
}