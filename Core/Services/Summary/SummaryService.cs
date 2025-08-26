/*using System.Collections.ObjectModel;
using Data.Repositories.Summary;
using Models.EquipmentTree;
using Models.Summary.ColumnTree;
using Models.Summary.DataGrid;
using Syncfusion.UI.Xaml.Grid;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Models.Equipment;
using Models.Equipment.ColumnSettings;
using Newtonsoft.Json;
using NuGet;
using Syncfusion.XPS;
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
    
    /*
    public async Task<SummaryFormat> GetSummaryFormat(int summaryId)
    {
       return await _summaryRepository.GetSummaryFormat(summaryId);
    }
    #1#

    /*public async Task<ObservableCollection<ISummaryFileSystemItem>> GetHierarchicalItemsAsync(int summaryId, SummaryFormat format)
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
    }#1#
    
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

    public async Task<List<DuplicateColumnInfo>> GetPotentialDuplicateColumnInfosAsync(int summaryId)
    {
        var allSelectedColumnsMetadata = await _summaryRepository.GetEquipmentReportColumnsMetadata(summaryId);
        var potentialDuplicates = new List<DuplicateColumnInfo>();
        var uniqueColumnKeysForComparison  = new Dictionary<string, ReportColumnMetadata>();

        foreach (var currentMetadata in allSelectedColumnsMetadata)
        {
            bool shooldConsiderForAsking = !currentMetadata.MergedIntoColumnId.HasValue && !currentMetadata.IsMergeDecisionMade.HasValue;
            
            if (!shooldConsiderForAsking)
            {
                continue;
            }
            
            var columnUniqueKey = currentMetadata.HeaderText;

            if (uniqueColumnKeysForComparison .TryGetValue(columnUniqueKey, out var existingColumn))
            {
                potentialDuplicates.Add(new DuplicateColumnInfo
                {
                    ExistingColumn = existingColumn,
                    DuplicateColumn = currentMetadata
                });
            }
            else
            {
                uniqueColumnKeysForComparison .Add(columnUniqueKey, currentMetadata);
            }
        }
        return potentialDuplicates;
    }

    public async Task<bool> ResolveDuplicateAndNotifyUserAsync(List<DuplicateColumnInfo> duplicatesToResolve)
    {
        bool changesMade = false;
        foreach (var duplicateInfo in duplicatesToResolve)
        {
            string message = $"У звіті виявлено дві характеристики з однаковим заголовком: \n\n" +
                             $"- Вихідна характеристика: '{duplicateInfo.ExistingColumn.HeaderText}' (з листа '{duplicateInfo.ExistingColumn.EquipmentSheetName}')\n" +
                             $"- Дублікат: '{duplicateInfo.DuplicateColumn.HeaderText}' (з листа '{duplicateInfo.DuplicateColumn.EquipmentSheetName}')\n\n" +
                             $"Об'єднати в одну характеристику?";
            MessageBoxResult result = MessageBox.Show(message, "Об'єднання значень", MessageBoxButton.YesNo, MessageBoxImage.Question);
            bool userAcceptedMerge = (result == MessageBoxResult.Yes);

            if (userAcceptedMerge)
            {
                await _summaryRepository.UpdateEquipmentSummaryMergedStatus(duplicateInfo.DuplicateColumn.EquipmentSummaryEntryId, duplicateInfo.ExistingColumn.CustomColumnId, true, true);
                changesMade = true;
            }
            else
            {
                await _summaryRepository.UpdateEquipmentSummaryMergedStatus(
                    duplicateInfo.DuplicateColumn.EquipmentSummaryEntryId,
                    null, 
                    false, 
                    true); 
                changesMade = true;
            }
        }

        return changesMade;
    }

    private List<InternalGridColumnDefinition> GetProcessedInternalColumns(List<ReportColumnMetadata> allSelectedColumnsMetadata)
    {
        var displayColumnsInternal = new List<InternalGridColumnDefinition>();
        var targetColumnMap = new Dictionary<int, InternalGridColumnDefinition>();
        var uniqueHeaderTextMap = new Dictionary<string, InternalGridColumnDefinition>();

        foreach (var currentMetadata in allSelectedColumnsMetadata.Where(m => !m.MergedIntoColumnId.HasValue))
        {
            var columnUniqueKey = currentMetadata.HeaderText;

            if (!uniqueHeaderTextMap.ContainsKey(columnUniqueKey))
            {
                var newInternalColumn = new InternalGridColumnDefinition
                {
                    CustomColumnId = currentMetadata.CustomColumnId,
                    HeaderText = currentMetadata.HeaderText,
                    MappingName = currentMetadata.MappingName,
                    EquipmentSheetName = currentMetadata.EquipmentSheetName,
                    CustomTableId = currentMetadata.TableId,
                    OriginalCustomColumnIds = new List<int> { currentMetadata.CustomColumnId },
                    OriginalMappingNames = new List<string> { currentMetadata.MappingName },
                    IsMergedTarget = false,
                    ColumnSettings = currentMetadata.ColumnSettings
                };
                displayColumnsInternal.Add(newInternalColumn);
                targetColumnMap.Add(newInternalColumn.CustomColumnId, newInternalColumn);
                uniqueHeaderTextMap.Add(columnUniqueKey, newInternalColumn);
            }
            else
            {
                var newInternalColumn = new InternalGridColumnDefinition
                {
                    CustomColumnId = currentMetadata.CustomColumnId,
                    HeaderText = currentMetadata.HeaderText,
                    MappingName = currentMetadata.MappingName,
                    EquipmentSheetName = currentMetadata.EquipmentSheetName,
                    CustomTableId = currentMetadata.TableId,
                    OriginalCustomColumnIds = new List<int> { currentMetadata.CustomColumnId },
                    OriginalMappingNames = new List<string> { currentMetadata.MappingName },
                    IsMergedTarget = false,
                    ColumnSettings = currentMetadata.ColumnSettings
                };
                displayColumnsInternal.Add(newInternalColumn);
                targetColumnMap.Add(newInternalColumn.CustomColumnId, newInternalColumn);
            }
        }

        foreach (var currentMetadata in allSelectedColumnsMetadata.Where(m => m.MergedIntoColumnId.HasValue))
        {
            if (targetColumnMap.TryGetValue(currentMetadata.MergedIntoColumnId.Value, out var targetColumn))
            {
                targetColumn.OriginalCustomColumnIds.Add(currentMetadata.CustomColumnId);
                targetColumn.OriginalMappingNames.Add(currentMetadata.MappingName);
                targetColumn.IsMergedTarget = true;
            }
        }

        return displayColumnsInternal;
    }


    public async Task<List<ReportGridColumn>> GetReportGridColumnsAsync(int summaryId, CancellationToken ct = default)
    {
        var allSelectedColumnsMetadata = await _summaryRepository.GetEquipmentReportColumnsMetadata(summaryId, ct);
        var processedInternalColumns = GetProcessedInternalColumns(allSelectedColumnsMetadata);

        return processedInternalColumns.Select(c => new ReportGridColumn
        {
            HeaderText = c.HeaderText,
            MappingName = c.MappingName,
            ColumnSettings = c.ColumnSettings
        }).ToList();
    }

    public async Task<List<ReportStackedHeaderRowDefinition>> GetStackedHeaderRowsDefinitionsAsync(int summaryId, CancellationToken ct = default)
    {
        var allSelectedColumnsMetadata = await _summaryRepository.GetEquipmentReportColumnsMetadata(summaryId, ct);
        var processedInternalColumns = GetProcessedInternalColumns(allSelectedColumnsMetadata);

        var finalStackedHeaderRows = new List<ReportStackedHeaderRowDefinition>();

        // Группируем колонки по имени листа (EquipmentSheetName)
        var groupedNonMergedColumns = processedInternalColumns
            .Where(c => !c.IsMergedTarget)
            .GroupBy(c => c.EquipmentSheetName);

        foreach (var sheetGroup in groupedNonMergedColumns)
        {
            var stackedRowDefinition = new ReportStackedHeaderRowDefinition();

            var stackedColumnDefinition = new ReportStackedHeaderColumnDefinition
            {
                HeaderText = sheetGroup.Key,
                MappingName = $"Sheet_{sheetGroup.Key.Replace(" ", "")}_Details",
                ChildColumnMappingNames = sheetGroup.Select(c => c.MappingName).ToList()
            };

            stackedRowDefinition.StackedColumns.Add(stackedColumnDefinition);
            finalStackedHeaderRows.Add(stackedRowDefinition);
        }

        return finalStackedHeaderRows;
    }

    public async Task<ObservableCollection<ExpandoObject>> GetReportItemsAsync(int summaryId, CancellationToken ct = default)
    {
       var allSelectedColumnsMetadata = await _summaryRepository.GetEquipmentReportColumnsMetadata(summaryId, ct);
       var internalProcessedColumns = GetProcessedInternalColumns(allSelectedColumnsMetadata);
       var customTableIdsToFetchData = allSelectedColumnsMetadata.Select(m => m.TableId).Distinct().ToList();

       if (!customTableIdsToFetchData.Any())
           return new ObservableCollection<ExpandoObject>();
       
       var allRawData = await _summaryRepository.GetRawDataForEquipmentsAsync(customTableIdsToFetchData, ct);

       var list = new List<ExpandoObject>();

       foreach (var rawDataEntry in allRawData)
       {
           ct.ThrowIfCancellationRequested();

           dynamic row = new ExpandoObject();
           var rowDictionary = (IDictionary<string, object>)row;

           var parsedJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawDataEntry.RowDataJsonb);
           if (parsedJson is null) continue;

           foreach (var colDef in internalProcessedColumns)
               rowDictionary[colDef.MappingName] = null;

           foreach (var colDef in internalProcessedColumns)
           {
               object? value = null;

               if (colDef.IsMergedTarget)
               {
                   var merged = colDef.OriginalMappingNames
                       .Where(parsedJson.ContainsKey)
                       .Select(n => parsedJson[n])
                       .Where(v => v != null)
                       .ToList();
                   value = AggregateValuesForMergedColumn(merged);
               }
               else
               {
                   if (parsedJson.TryGetValue(colDef.OriginalMappingNames.FirstOrDefault() ?? "", out var v))
                       value = v;
               }

               rowDictionary[colDef.MappingName] = value;
           }

           list.Add(row);
       }
       return new ObservableCollection<ExpandoObject>(list);
    }
    
    private object AggregateValuesForMergedColumn(List<object> values)
    {
        if (!values.Any())
        {
            return null;
        }
        var stringValues = values.Where(v => v != null)
            .Select(v => v.ToString())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        if (stringValues.Any())
        {
            return string.Join(", ", stringValues);
        }
        return null;
    }
        
    private class InternalGridColumnDefinition
    {
        public int CustomColumnId { get; set; }
        public string HeaderText { get; set; } 
        public string MappingName { get; set; } 
        public string EquipmentSheetName { get; set; }
        public int CustomTableId { get; set; }
        public List<int> OriginalCustomColumnIds { get; set; } = new List<int>();
        public List<string> OriginalMappingNames { get; set; } = new List<string>();
        public bool IsMergedTarget { get; set; } = false;
        public ColumnSettingsDisplayModel ColumnSettings { get; set; } = new();
    }
}*/