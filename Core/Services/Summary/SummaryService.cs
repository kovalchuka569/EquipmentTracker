using System.Collections.ObjectModel;
using Data.Repositories.Summary;
using Models.EquipmentTree;
using Models.Summary.ColumnTree;
using Models.Summary.DataGrid;
using Syncfusion.UI.Xaml.Grid;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Core.Services.EquipmentDataGrid;
using Models.Equipment;
using NuGet;
using ColumnDto = Models.Summary.ColumnTree.ColumnDto;
using ColumnItem = Models.Summary.ColumnTree.ColumnItem;
using FileDto = Models.Summary.ColumnTree.FileDto;
using FileItem = Models.Summary.ColumnTree.FileItem;
using FolderItem = Models.Summary.ColumnTree.FolderItem;

namespace Core.Services.Summary;

public class SummaryService : ISummaryService
{
    private readonly ISummaryRepository _summaryRepository;

    public SummaryService(ISummaryRepository summaryRepository)
    {
        _summaryRepository = summaryRepository;
    }
    
    public async Task<SummaryFormat> GetSummaryFormat(int summaryId)
    {
       return await _summaryRepository.GetSummaryFormat(summaryId);
    }

    public async Task<ObservableCollection<ISummaryFileSystemItem>> GetHierarchicalItemsAsync(int summaryId, SummaryFormat format)
    {
        var folders = await _summaryRepository.GetFoldersAsync();
        var files = new List<FileDto>();
        switch (format)
        {
            case SummaryFormat.EquipmentsSummary:
                files = await _summaryRepository.GetEquipmentFilesAsync();
                break;
        }
        var colDict = new Dictionary<int, List<ColumnDto>>();

        foreach (var file in files)
        {
            colDict[file.Id] = await _summaryRepository.GetColumnsForTreeAsync(file.TableId);
        }

        var folderItems = folders.Select(f => new FolderItem
        {
            Id = f.Id,
            Name = f.Name,
            ParentId = f.ParentId ?? 0,
            Children = new ObservableCollection<ISummaryFileSystemItem>(),
            ImageIcon = "Assets/folder.png"
        }).ToDictionary(f => f.Id);
        
        var fileItems = files.Select(f => new FileItem
        {
            Id = f.Id,
            Name = f.Name,
            ParentId = f.FolderId ?? 0,
            Children = new ObservableCollection<ISummaryFileSystemItem>(),
            ImageIcon = "Assets/file.png"
        }).ToDictionary(f => f.Id);

        foreach (var kv in colDict)
        {
            var colModels = kv.Value.Select(c => new ColumnItem
            {
                ColumnId = c.Id,
                ParentFileId = kv.Key,
                Name = c.HeaderText,
                ImageIcon = "Assets/column.png"
            }).ToList();
            
            if (fileItems.TryGetValue(kv.Key, out var parentFile))
            {
                foreach (var col in colModels)
                {
                    parentFile.Children.Add(col);
                }
            }
        }

        foreach (var file in fileItems.Values)
        {
            if (folderItems.TryGetValue(file.ParentId, out var parent))
            {
                parent.Children.Add(file);
            }
        }

        foreach (var folder in folderItems.Values)
        {
            if (folder.ParentId != 0 && folderItems.TryGetValue(folder.ParentId, out var parent))
            {
                parent.Children.Add(folder);
            }
        }
        return new ObservableCollection<ISummaryFileSystemItem>(
            folderItems.Values.Where(f => f.ParentId == 0));
    }

    public async Task<Columns> GetMergedColumnsAsync(int summaryId,SummaryFormat format)
    {
        var columnIds = await _summaryRepository.GetEquipmentSelectedColumnsIds(summaryId);
        var cellTemplate = new CreateCellTemplateFactory();
        var columnItems = new List<global::Models.Summary.DataGrid.ColumnItem>();
        switch (format)
        {
            case SummaryFormat.EquipmentsSummary:
                columnItems = await _summaryRepository.GetColumnItemsForEquipmentsAsync(columnIds);
                //await _summaryRepository.InsertEquipmentSummary(columnIds, summaryId);
                break;
        }
        
        var columns = new Columns();
        
        foreach (var colItem in columnItems)
        {
            var gridColumn = new CustomGridTemplateColumn
            {
                Id = colItem.Id,
                HeaderText = colItem.ColumnSettings.HeaderText,
                MappingName = colItem.ColumnSettings.MappingName,
                TableId = colItem.TableId,
                CellTemplate = cellTemplate.CreateCellTemplate(colItem.ColumnSettings),
                ColumnMemberType = GetColumnMemberType(colItem.ColumnSettings.DataType)
            };
            columns.Add(gridColumn);
        }
        
        return MergeColumns(columns);
    }

    private static Columns MergeColumns(Columns columns)
    {
        var grouped = columns.GroupBy(c => c.HeaderText).ToList();
        var result = new Columns();

        foreach (var group in grouped)
        {
            if (group.Count() > 1)
            {
                var message = $"Знайденно дублювання характеристики: \"{group.Key}\". Об'єднати значення?";
                var resultMessage = MessageBox.Show(message, "Об'єднання значень", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (resultMessage == MessageBoxResult.Yes)
                {
                    result.Add(group.First());
                }
                else
                {
                    result.AddRange(group);
                }
            }
            else
            {
                result.Add(group.First());
            }
        }

        return result;
    }

    private static Type GetColumnMemberType(ColumnDataType dataType)
    {
        switch (dataType)
        {
            case ColumnDataType.Date:
                return typeof(DateTime);
            case ColumnDataType.Boolean:
                return typeof(bool);
            case ColumnDataType.Currency:
                return typeof(double);
            case ColumnDataType.Hyperlink:
                return typeof(string);
            case ColumnDataType.List:
                return typeof(string);
            case ColumnDataType.Number:
                return typeof(double);
            case ColumnDataType.Text:
                return typeof(string);
            case ColumnDataType.MultilineText:
                return typeof(string);
        }
        return typeof(string);
    }

    public async Task<ObservableCollection<ExpandoObject>> GetDataAsync(List<int> tableIds, SummaryFormat format)
    {
        var rawRows = new List<Dictionary<string, object>>();
        switch (format)
        {
            case SummaryFormat.EquipmentsSummary:
                rawRows = await _summaryRepository.GetDataForEquipmentsAsync(tableIds);
                break;
        }
        var collection = new ObservableCollection<ExpandoObject>();
        foreach (var dictRow in rawRows)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = (IDictionary<string, object>)expando;
            foreach (var kv in dictRow)
            {
                expandoDict[kv.Key] = kv.Value;
            }
            collection.Add(expando);
        }
        return collection;
    }

    public async Task<List<(string, string)>> GetTableNamesForEquipmentSummaryAsync(List<int> columnIds)
    {
       return await _summaryRepository.GetTableNamesForEquipmentSummaryAsync(columnIds);
    }
    
    public void SelectItemAndChildren(ISummaryFileSystemItem? item)
    {
        item.IsSelected = true;

        if (item.Children != null)
        {
            foreach (var child in item.Children)
            {
                SelectItemAndChildren(child);
            }
        }
    }
    
    public void UnselectItemAndChildren(ISummaryFileSystemItem? item)
    {
        item.IsSelected = false;

        if (item.Children != null)
        {
            foreach (var child in item.Children)
            {
                UnselectItemAndChildren(child);
            }
        }
    }
    
    public void UpdateParentSelectionStates(ObservableCollection<ISummaryFileSystemItem> items)
    {
        foreach (var item in items)
        {
            if (item.Children != null && item.Children.Count > 0)
            {
                UpdateParentSelectionStates(item.Children);

                bool allTrue = item.Children.All(c => c.IsSelected == true);
                bool allFalse = item.Children.All(c => c.IsSelected == false);

                if (allTrue)
                    item.IsSelected = true;
                else if (allFalse)
                    item.IsSelected = false;
                else
                    item.IsSelected = null;
            }
        }
    }

    public void SelectInCollectionById(ObservableCollection<ISummaryFileSystemItem>? items, List<int> columnIds)
    {
        if (items == null || columnIds == null) return;

        var idSet = new HashSet<int>(columnIds);

        foreach (var item in items)
        {
            if (item is ColumnItem columnItem)
            {
                columnItem.IsSelected = idSet.Contains(columnItem.ColumnId);
            }
            SelectInCollectionById(item.Children, columnIds);
        }
    }

    public List<int> GetAllSelectedColumnIds(ObservableCollection<ISummaryFileSystemItem> items)
    {
        var ids = new List<int>();

        foreach (var item in items)
        {
            if (item.IsSelected == true && item is ColumnItem column)
            {
                ids.Add(column.ColumnId);
            }

            if (item.Children != null)
            {
                ids.AddRange(GetAllSelectedColumnIds(item.Children));
            }
        }

        return ids;
    }

    public async Task UpdateSelectedSummaryColumnsIds (int summaryId, List<int> columnIds, CheckboxAction action, SummaryFormat format)
    {
        var dbColumnIds = new List<int>();
        switch (format)
        {
            case SummaryFormat.EquipmentsSummary:
                dbColumnIds = await _summaryRepository.GetEquipmentSelectedColumnsIds(summaryId);
                var differenceIds = GetDifferenceIds(dbColumnIds, columnIds, action);
                switch (action)
                {
                    case CheckboxAction.Check:
                        await _summaryRepository.InsertEquipmentSummaryColumnAsync(differenceIds, summaryId);
                        break;
                    case CheckboxAction.Uncheck:
                        await _summaryRepository.DeleteEquipmentSummaryColumnAsync(differenceIds, summaryId);
                        break;
                }
                break;
        }
    }

    private List<int> GetDifferenceIds(List<int> dbColumns, List<int> selectedColumns, CheckboxAction action)
    {
        return action switch
        {
            CheckboxAction.Check => selectedColumns.Except(dbColumns).ToList(),
            CheckboxAction.Uncheck => dbColumns.Except(selectedColumns).ToList(),
            _ => new List<int>()
        };
    }

    public async Task<List<int>> GetEquipmentSelectedColumnsIds(int summaryId, SummaryFormat format)
    {
        var result = new List<int>();
        switch (format)
        {
            case SummaryFormat.EquipmentsSummary:
                result = await _summaryRepository.GetEquipmentSelectedColumnsIds(summaryId);
                break;
            case SummaryFormat.RepairsSummary:
                break;
            case SummaryFormat.ServicesSummary:
                break;
            case SummaryFormat.WriteOffSummary:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        return result;
    }
}