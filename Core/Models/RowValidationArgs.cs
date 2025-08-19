using Models.Common.Table;
using Models.Common.Table.ColumnValidationRules;
using Models.Equipment;

namespace Core.Models;

public class RowValidationArgs
{
    /// <summary>
    /// Current domain row model.
    /// </summary>
    public RowModel CurrentRow { get; set; }
    
    /// <summary>
    /// Dictionary of column mapping name to id.
    /// </summary>
    public Dictionary<string, Guid> ColumnMappingNameIdMap { get; set; } = new();
    
    /// <summary>
    /// Dictionary of column id to header text mapping.
    /// </summary>
    public Dictionary<Guid, string> ColumnIdHeaderTextMap { get; set; } = new();
    
    /// <summary>
    /// Dictionary of column id to data type.
    /// </summary>
    public Dictionary<Guid, ColumnDataType> ColumnIdDataTypeMap { get; set; } = new();
    
    /// <summary>
    /// Dictionary of column id to validation rules.
    /// </summary>
    public Dictionary<Guid, IColumnValidationRules> ColumnIdValidationRulesMap { get; set; } = new();
    
    /// <summary>
    /// List of all rows.
    /// </summary>
    public List<RowModel> Rows { get; set; } = new();
}