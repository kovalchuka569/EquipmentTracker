using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Prism.Mvvm;

using Models.Common.Table;

namespace Presentation.ViewModels.Common.Table;

public class RowViewModel : BindableBase
{
    private bool _isUpdating;
    
    private bool _isNew = true;

    private Guid _id = Guid.NewGuid();

    private int _position;

    private bool _deleted;

    private Dictionary<string, CellViewModel> _cellsByMappingName = new();

    private Dictionary<Guid, CellViewModel> _cellsById = new();

    /// <summary>
    /// Is the new row or existing.
    /// </summary>
    public bool IsNew
    {
        get => _isNew;
        set => SetProperty(ref _isNew, value);
    }

    /// <summary>
    /// Row ID.
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// Row position.
    /// </summary>
    public int Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    /// <summary>
    /// Row deleted status.
    /// </summary>
    public bool Deleted
    {
        get => _deleted;
        set => SetProperty(ref _deleted, value);
    }

    /// <summary>
    /// Cells by mapping name dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, CellViewModel> CellsByMappingName => _cellsByMappingName;
    
    /// <summary>
    /// Cells by ID dictionary.
    /// </summary>
    public IReadOnlyDictionary<Guid, CellViewModel> CellsById => _cellsById;

    /// <summary>
    /// Gets cell by mapping name.
    /// </summary>
    /// <param name="mappingName">Column mapping name.</param>
    /// <param name="cell">Cell view model.</param>
    /// <returns>
    /// True and existing cell view model if cell exists, false and null CellViewModel otherwise.
    /// </returns>
    public bool TryGetCellByMappingName(string mappingName, out CellViewModel? cell)
    {
        if (!_cellsByMappingName.TryGetValue(mappingName, out cell)) return false;
        
        cell.PropertyChanged += OnCellPropertyChanged;
        return true;
    }
    
    /// <summary>
    /// Gets cell by ID.
    /// </summary>
    /// <param name="cellId">Cell ID.</param>
    /// <param name="cell">Cell view model.</param>
    /// <returns>
    /// True and existing cell view model if cell exists, false and null CellViewModel otherwise.
    /// </returns>
    public bool TryGetCellById(Guid cellId, out CellViewModel? cell)
    {
        if (!_cellsById.TryGetValue(cellId, out cell)) return false;

        cell.PropertyChanged += OnCellPropertyChanged;
        return true;
    }

    /// <summary>
    /// Get the value of cell by mapping name.
    /// </summary>
    /// <param name="mappingName">Column mapping name.</param>
    /// <param name="cellValue">Output value of cell.</param>
    /// <returns>
    /// True and cell value if cell exists, false and null value otherwise.
    /// </returns>
    public bool TryGetCellValueByMappingName(string mappingName, out object? cellValue)
    {
        if (_cellsByMappingName.TryGetValue(mappingName, out var cell))
        {
            cellValue = cell.Value;
            return true;
        }
        
        cellValue = null;
        return false;
    }

    /// <summary>
    /// Gets cell value by ID.
    /// </summary>
    /// <param name="id">Cell ID.</param>
    /// <param name="cellValue">Output value of cell</param>
    /// <returns>True and cell value if cell exists, false and null value otherwise.</returns>
    public bool TryGetCellValueById(Guid id, out object? cellValue)
    {
        if (_cellsById.TryGetValue(id, out var cell))
        {
            cellValue = cell.Value;
            return true;
        }
        
        cellValue = null;
        return false;
    }

    /// <summary>
    /// Set the value of cell by mapping name.
    /// </summary>
    /// <param name="mappingName">Mapping name.</param>
    /// <param name="newValue">New value.</param>
    /// <returns>
    /// True if cell exists, false otherwise.
    /// </returns>
    public bool TrySetCellValueByMappingName(string mappingName, object? newValue)
    {
        if (!_cellsByMappingName.TryGetValue(mappingName, out var cell)) return false;
        
        cell.Value = newValue;
        return true;
    }

    /// <summary>
    /// Set the value of cell by cell ID.
    /// </summary>
    /// <param name="cellId">Cell ID.</param>
    /// <param name="newValue">New value.</param>
    /// <returns>
    /// True if cell exists, false otherwise.
    /// </returns>
    public bool TrySetCellValueById(Guid cellId, object? newValue)
    {
        if (!_cellsById.TryGetValue(cellId, out var cell)) return false;
        
        cell.Value = newValue;
        return true;
    }

    /// <summary>
    /// Adds a cell to the row.
    /// </summary>
    /// <param name="cellViewModel">Cell view model.</param>
    public void AddCell(CellViewModel cellViewModel)
    {
        _cellsByMappingName[cellViewModel.ColumnMappingName] = cellViewModel;
        _cellsById[cellViewModel.Id] = cellViewModel;
        cellViewModel.PropertyChanged += OnCellPropertyChanged;
    }

    /// <summary>
    /// Attempts to add a cell to the row.
    /// </summary>
    /// <param name="cellViewModel">Cell view model.</param>
    /// <returns>
    /// True if the cell was added to the row successfully; otherwise, false.
    /// </returns>
    public bool TryAddCell(CellViewModel? cellViewModel)
    {
        if(cellViewModel is null || string.IsNullOrWhiteSpace(cellViewModel.ColumnMappingName)) return false;
        
        if(!_cellsByMappingName.TryAdd(cellViewModel.ColumnMappingName, cellViewModel)) return false;

        if (!_cellsById.TryAdd(cellViewModel.Id, cellViewModel))
        { 
            _cellsByMappingName.Remove(cellViewModel.ColumnMappingName); 
            return false;   
        }
        
        cellViewModel.PropertyChanged += OnCellPropertyChanged;
        return true;
    }

    /// <summary>
    /// Converts the row domain model to the row view model.
    /// </summary>
    /// <param name="rowModel">Row domain model.</param>
    /// <returns>Row view model.</returns>
    public static RowViewModel FromDomain(RowModel rowModel)
    {
        var rowViewModel = new RowViewModel
        {
            Id = rowModel.Id,
            Position = rowModel.Position,
            Deleted = rowModel.Deleted,
            IsNew = false
        };

        foreach (var cellViewModel in rowModel.Cells.Select(CellViewModel.FromDomain))
        {
            rowViewModel.AddCell(cellViewModel);
        }

        return rowViewModel;
    }


    /// <summary>
    /// Attempts to convert the row domain model to the row view model.
    /// </summary>
    /// <param name="rowModel">Row domain model.</param>
    /// <param name="rowViewModel">Row view model.</param>
    /// <returns>
    /// True if the row view model was created successfully; otherwise, false.
    /// </returns>
    public static bool TryFromDomain(RowModel? rowModel, out RowViewModel? rowViewModel)
    {
        if (rowModel is null)
        {
            rowViewModel = null;
            return false;
        }

        try
        {
            var newRowViewModel = new RowViewModel
            {
                Id = rowModel.Id,
                Position = rowModel.Position,
                Deleted = rowModel.Deleted,
                IsNew = false
            };

            foreach (var cellMode in rowModel.Cells)
            {
                if (!CellViewModel.TryFromDomain(cellMode, out var cellViewModel))
                {
                    rowViewModel = null;
                    return false;
                }

                if (!newRowViewModel.TryAddCell(cellViewModel))
                {
                    rowViewModel = null;
                    return false;
                }
            }
            
            rowViewModel = newRowViewModel;
            return true;
        }
        catch
        {
            rowViewModel = null;
            return false;
        }
    }

    /// <summary>
    /// Converts the row view model to the row domain model.
    /// </summary>
    /// <param name="rowViewModel">Row view model.</param>
    /// <returns>Row domain model.</returns>
    public static RowModel ToDomain(RowViewModel rowViewModel)
    {
        return new RowModel
        {
            Id = rowViewModel.Id,
            Position = rowViewModel.Position,
            Deleted = rowViewModel.Deleted,
            Cells = rowViewModel.CellsByMappingName.Values
                .Select(CellViewModel.ToDomain)
                .ToList()
        };
    }
    
    private void OnCellPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(_isUpdating) return;

        if (sender is not CellViewModel cell || e.PropertyName != nameof(CellViewModel.Value)) return;
        
        try
        {
            _isUpdating = true;
            RaisePropertyChanged(cell.ColumnMappingName);
        }
        finally
        {
            _isUpdating = false;
        }
    }

}