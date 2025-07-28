using System.Collections.ObjectModel;
using Models.EquipmentTree;
using System.Dynamic;
using Models.Summary.ColumnTree;
using Models.Summary.DataGrid;
using Syncfusion.UI.Xaml.Grid;

namespace Core.Services.Summary;

public interface ISummaryService
{
    /// <summary>
    /// Get summary format for a given summary ID.
    /// </summary>
    /// <param name="summaryId">Summary ID</param>
    /// <returns>Summary format</returns>
    Task<SummaryFormat> GetSummaryFormat(int summaryId);
    
    /// <summary>
    /// Depending on summary format builds a hierarchical collection for a tree of columns that are not present in the report table.
    /// </summary>
    /// <returns>Hierarchical collection of a files, folders, columns</returns>
    Task<ObservableCollection<ISummaryFileSystemItem>> GetHierarchicalItemsAsync(int summaryId, SummaryFormat format);

    /// <summary>
    /// Selects an element and its children
    /// </summary>
    /// <param name="item">Any element of the tree</param>
    void SelectItemAndChildren (ISummaryFileSystemItem? item);
    
    /// <summary>
    /// Unselects an element and its children
    /// </summary>
    /// <param name="item">Any element of the tree</param>
    void  UnselectItemAndChildren (ISummaryFileSystemItem? item);
    
    void UpdateParentSelectionStates (ObservableCollection<ISummaryFileSystemItem> items);
    
    /// <summary>
    /// Selected items in collection by id
    /// </summary>
    /// <param name="items">Items collection</param>
    /// <param name="ids">IDs list</param>
    void SelectInCollectionById (ObservableCollection<ISummaryFileSystemItem> items, List<int> ids);

    /// <summary>
    /// Gets all selected column ids
    /// </summary>
    /// <param name="items">ItemSource collection of the tree</param>
    /// <returns>List of selected column ids</returns>
    List<int> GetAllSelectedColumnIds(ObservableCollection<ISummaryFileSystemItem> items);

    /// <summary>
    /// Update selected summary columns ids 
    /// </summary>
    /// <param name="summaryId">Summary ID</param>
    /// <param name="columnIds">List of IDs selected columns</param>
    /// <param name="action">Checkbox action</param>
    /// <param name="format">Summary format</param>
    Task UpdateSelectedSummaryColumnsIds (int summaryId, List<int> columnIds, CheckboxAction action, SummaryFormat format);
    
    /// <summary>
    /// Gets all selected column ids
    /// </summary>
    /// <param name="summaryId">Summary ID</param>
    /// <param name="format">Summary format</param>
    /// <returns>List of selected columns ids</returns>
    Task<List<int>> GetEquipmentSelectedColumnsIds (int summaryId, SummaryFormat format);

    Task<List<DuplicateColumnInfo>> GetPotentialDuplicateColumnInfosAsync(int summaryId);
    Task<bool> ResolveDuplicateAndNotifyUserAsync(List<DuplicateColumnInfo> duplicatesToResolve);
    Task<List<ReportGridColumn>> GetReportGridColumnsAsync(int summaryId, CancellationToken ct = default);
    Task<ObservableCollection<ExpandoObject>> GetReportItemsAsync(int summaryId, CancellationToken ct = default);
    Task<List<ReportStackedHeaderRowDefinition>> GetStackedHeaderRowsDefinitionsAsync(int summaryId, CancellationToken ct = default);
}