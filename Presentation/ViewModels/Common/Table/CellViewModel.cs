using System;
using Prism.Mvvm;

using Models.Common.Table;

namespace Presentation.ViewModels.Common.Table;

public class CellViewModel : BindableBase
{
    private bool _isNew = true;
    
    private Guid _id = Guid.NewGuid();
    
    private Guid _rowId;
    
    private string _columnMappingName = string.Empty;
    
    private bool _deleted;
    
    private object? _value;

    /// <summary>
    /// Indicate is the new cell or existing.
    /// </summary>
    public bool IsNew
    {
        get => _isNew;
        set => SetProperty(ref _isNew, value);
    }
    
    /// <summary>
    /// Cell ID.
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// The row ID in which this cell is located
    /// </summary>
    public Guid RowId
    {
        get => _rowId;
        set => SetProperty(ref _rowId, value);
    }
    
    /// <summary>
    /// Cell column mapping name.
    /// </summary>
    public string ColumnMappingName
    {
        get => _columnMappingName;
        set => SetProperty(ref _columnMappingName, value);
    }
    
    /// <summary>
    /// Cell deleted status.
    /// </summary>
    public bool Deleted
    {
        get => _deleted;
        set => SetProperty(ref _deleted, value);
    }
    
    /// <summary>
    /// Cell value.
    /// </summary>
    public object? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
    
    /// <summary>
    /// Convert cell model to cell view model.
    /// </summary>
    /// <param name="cellModel">Cell model.</param>
    /// <returns>Cell view model.</returns>
    public static CellViewModel FromDomain(CellModel cellModel)
    {
        return new CellViewModel
        {
            Id = cellModel.Id,
            ColumnMappingName = cellModel.ColumnMappingName,
            Value = cellModel.Value,
            Deleted = cellModel.Deleted,
            IsNew = false
        };
    }

    /// <summary>
    /// Attempt to convert cell model to cell view model.
    /// </summary>
    /// <param name="cellModel">Cell domain model.</param>
    /// <param name="cellViewModel">Cell view model.</param>
    /// <returns>
    /// True if cell view model was created successfully, false otherwise.
    /// </returns>
    public static bool TryFromDomain(CellModel? cellModel, out CellViewModel? cellViewModel)
    {
        if (cellModel is null)
        {
            cellViewModel = null;
            return false;
        }


        var newCellViewModel = new CellViewModel
        {
            Id = cellModel.Id,
            RowId = cellModel.RowId,
            ColumnMappingName = cellModel.ColumnMappingName,
            Value = cellModel.Value,
            Deleted = cellModel.Deleted,
        };
        cellViewModel = newCellViewModel;
        return true;
    }

    /// <summary>
    /// Convert cell view model to cell model.
    /// </summary>
    /// <param name="cellViewModel">Cell view model.</param>
    /// <returns>Cell model.</returns>
    public static CellModel ToDomain(CellViewModel cellViewModel)
    {
        return new CellModel
        {
            Id = cellViewModel.Id,
            RowId = cellViewModel.RowId,
            ColumnMappingName = cellViewModel.ColumnMappingName,
            Value = cellViewModel.Value,
            Deleted = cellViewModel.Deleted
        };
    }
}