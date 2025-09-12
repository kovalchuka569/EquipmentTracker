using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Prism.Commands;
using Prism.Dialogs;
using Notification.Wpf;
using Core.Interfaces;
using Common.Logging;
using JetBrains.Annotations;
using Models.Common.Table.ColumnProperties;
using Models.Equipment;
using Presentation.Attributes;
using Presentation.EventArgs;
using Presentation.Models;
using Presentation.ViewModels.Common.ColumnDesigner;

using DescriptionAttribute = Presentation.Attributes.DescriptionAttribute;
using DisplayNameAttribute = Presentation.Attributes.DisplayNameAttribute;
using EditorAttribute = Presentation.Attributes.EditorAttribute;

namespace Presentation.ViewModels;

public class ColumnDesignerViewModel : BaseViewModel<ColumnDesignerViewModel>, IClosableDialog, IDialogAware
{
    
    #region Private fields

    private ObservableCollection<BaseColumnPropertiesViewModel> _columnProperties = new();
    private BaseColumnPropertiesViewModel? _selectedColumnProperties;
    private ObservableCollection<ColumnPropertyEntry> _selectedColumnPropertyEntries = new();
    private DelegateCommand<IDialogResult>? _closeDialogFromHostCommand;

    #endregion

    #region Public fields

    public ObservableCollection<BaseColumnPropertiesViewModel> ColumnProperties
    {
        get => _columnProperties;
        set => SetProperty(ref _columnProperties, value);
    }

    public BaseColumnPropertiesViewModel? SelectedColumnProperties
    {
        get => _selectedColumnProperties;
        set
        {
            Console.WriteLine("SelectedColumnProperties changed");
            if (_selectedColumnProperties is not null)
            {
                Console.WriteLine("_selectedcolumnproerties is not null (unsubscribe)");
                _selectedColumnProperties.ColumnDataTypeChanged -= OnSelectedColumnDataTypeChanged;
            }
            
            Console.WriteLine($"value: {value?.ColumnDataType}");

            _selectedColumnProperties = value;
            RaisePropertyChanged();
            
            if (_selectedColumnProperties is not null)
            {
                Console.WriteLine("_selectedcolumnproerties is not null (subscribe)");
                _selectedColumnProperties.ColumnDataTypeChanged += OnSelectedColumnDataTypeChanged;
            }
                
            RebuildEntries();
        }
    }

    public ObservableCollection<ColumnPropertyEntry> SelectedColumnPropertyEntries
    {
        get => _selectedColumnPropertyEntries;
        set => SetProperty(ref _selectedColumnPropertyEntries, value);
    }

    public bool HasPropertiesErrors => _columnProperties.Any(x => x.HasErrors);

    #endregion

    #region Constructor

    public ColumnDesignerViewModel(NotificationManager notificationManager, IAppLogger<ColumnDesignerViewModel> logger)
        : base(notificationManager, logger)
    {
        InitializeCommands();

        _columnProperties.CollectionChanged += ColumnProperties_CollectionChanged;
    }

    #endregion

    #region Commands

    public DelegateCommand CancelDialogCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand AddColumnCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand MarkForDeleteCommand
    {
        [UsedImplicitly]
        get;
        private set;
    } = null!;

    public DelegateCommand RemoveMarkForDeleteCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand SaveCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;

    public DelegateCommand<DragDropInfo> ColumnsListDropCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;
    
    public DelegateCommand<object> ColumnDataTypesOpenedCommand
    {
        [UsedImplicitly] get;
        private set;
    } = null!;
    
    #endregion

    #region Commands management

    private void InitializeCommands()
    {
        CancelDialogCommand = new DelegateCommand(OnDialogClosed);
        AddColumnCommand = new DelegateCommand(OnAddColumn);
        MarkForDeleteCommand = new DelegateCommand(OnMarkForDelete);
        RemoveMarkForDeleteCommand = new DelegateCommand(OnRemoveMarkForDelete);
        SaveCommand = new DelegateCommand(OnSave);
        ColumnsListDropCommand = new DelegateCommand<DragDropInfo>(OnColumnsListDrop);
        ColumnDataTypesOpenedCommand = new DelegateCommand<object>(OnColumnDataTypesOpened);
    }

    #endregion

    #region Private methods

    private void OnMarkForDelete()
    {
        if(SelectedColumnProperties is null)
            return;

        SelectedColumnProperties.MarkedForDelete = true;
    }
    
    private void OnRemoveMarkForDelete()
    {
        if(SelectedColumnProperties is null)
            return;

        SelectedColumnProperties.MarkedForDelete = false;
    }

    private void ColumnProperties_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if(e.OldItems is not null)
            foreach (var oldItem in e.OldItems.OfType<INotifyDataErrorInfo>())
                oldItem.ErrorsChanged -= OnColumnPropertiesErrorsChanged;

        if(e.NewItems is not null)
            foreach (var newItem in e.NewItems.OfType<INotifyDataErrorInfo>())
                newItem.ErrorsChanged += OnColumnPropertiesErrorsChanged;
    }
    
    private void OnColumnPropertiesErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(HasPropertiesErrors));
        Console.WriteLine(HasPropertiesErrors);
    }

    private void OnAddColumn()
    {
        var newColumnProperties = new TextColumnPropertiesViewModel
        {
            Order = ColumnProperties.Count
        };
        
        ColumnProperties.Add(newColumnProperties);
        SelectedColumnProperties = newColumnProperties;
    }

    private void RebuildEntries()
    {
        Console.WriteLine("Rebuild entries");
        if(SelectedColumnProperties is null)
            return;
        
        SelectedColumnPropertyEntries.Clear();
        
        var props = SelectedColumnProperties.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(pi => 
                pi.GetCustomAttribute<GroupNameAttribute>() != null ||
                pi.GetCustomAttribute<DisplayNameAttribute>() != null ||
                pi.GetCustomAttribute<DescriptionAttribute>() != null ||
                pi.GetCustomAttribute<EditorAttribute>() != null)
            .OrderBy(pi => pi.GetCustomAttribute<OrderAttribute>()?.Order 
                           ?? int.MaxValue)
            .ToList();

        foreach (var pi in props)
        {
            var entry = new ColumnPropertyEntry(SelectedColumnProperties, pi);
            SelectedColumnPropertyEntries.Add(entry);
        }
    }
    
    private void OnSelectedColumnDataTypeChanged(object? sender, ColumnDataTypeChangedEventArgs e)
    {
        
        if (sender is not BaseColumnPropertiesViewModel oldViewModel) 
            return;
        
        var oldIndex = ColumnProperties.IndexOf(oldViewModel);
        
        if (oldIndex == -1) 
            return;

        BaseColumnPropertiesViewModel newViewModel;
            
        switch (e.NewDataType)
        {
            case ColumnDataType.Number:
                newViewModel = new NumberColumnPropertiesViewModel();
                break;
            case ColumnDataType.Text:
                newViewModel = new TextColumnPropertiesViewModel();
                break;
            case ColumnDataType.Date:
                newViewModel = new DateColumnPropertiesViewModel();
                break;
            case ColumnDataType.Boolean:
                newViewModel = new BooleanColumnPropertiesViewModel();
                break;
            case ColumnDataType.Currency:
                newViewModel = new CurrencyColumnPropertiesViewModel();
                break;
            case ColumnDataType.List:
                newViewModel = new ListColumnPropertiesViewModel();
                break;
            case ColumnDataType.Hyperlink:
                newViewModel = new LinkColumnPropertiesViewModel();
                break;
            case ColumnDataType.None:
            default:
                return;
        }
            
        newViewModel.Id = oldViewModel.Id;
        newViewModel.MappingName = oldViewModel.MappingName;
        newViewModel.HeaderText = oldViewModel.HeaderText;
        newViewModel.HasDefaultValue = oldViewModel.HasDefaultValue;
        newViewModel.DefaultValue = oldViewModel.DefaultValue;
        newViewModel.HeaderWidth = oldViewModel.HeaderWidth;
        newViewModel.IsFrozen = oldViewModel.IsFrozen;
        newViewModel.IsUnique = oldViewModel.IsUnique;
        newViewModel.IsRequired = oldViewModel.IsRequired;
        newViewModel.Order = oldViewModel.Order;
        newViewModel.IsNew = oldViewModel.IsNew;
        newViewModel.IsModified = oldViewModel.IsModified;
        
        ColumnProperties[oldIndex] = newViewModel;
        SelectedColumnProperties = newViewModel;
    }

    private void OnColumnsListDrop(DragDropInfo e)
    {
        if (e.DroppedItem is not BaseColumnPropertiesViewModel droppedData)
            return;

        if(e.TargetItem is not BaseColumnPropertiesViewModel targetData)
            return;

        var oldIndex = ColumnProperties.IndexOf(droppedData);
        var newIndex = ColumnProperties.IndexOf(targetData);

        if (oldIndex < 0 || newIndex < 0 || oldIndex == newIndex)
            return;

        ColumnProperties.Move(oldIndex, newIndex);

        for (var i = 0; i < ColumnProperties.Count; i++)
        {
            ColumnProperties[i].Order = i;
        }
    }

    private void OnColumnDataTypesOpened(object parameter)
    {
        if(parameter is not BaseColumnPropertiesViewModel columnPropertiesViewModel)
            return;
        
        SelectedColumnProperties = columnPropertiesViewModel;
    }
    

    private void OnSave()
    {
        var newColumnProperties = ColumnProperties
            .Where(c => c.IsNew)
            .Select(c => c.ToDomain())
            .ToList();
        
        var editedColumnProperties = ColumnProperties
            .Where(c => c is { IsModified: true, IsNew: false })
            .Select(c => c.ToDomain())
            .ToList();
        
        var columnEditResult = new ColumnEditResult
        {
            EditedColumns = editedColumnProperties,
            NewColumns = newColumnProperties
        };
        var result = new DialogResult
        {
            Result = ButtonResult.OK,
            Parameters = new DialogParameters
            {
                {"ColumnEditResult", columnEditResult}
            }
        };
        _closeDialogFromHostCommand?.Execute(result);
    }
    
    #endregion

    #region IClosableDialog implementation

    public void SetCloseCommand(DelegateCommand<IDialogResult>? closeCommand)
    {
        _closeDialogFromHostCommand = closeCommand;
    }

    #endregion

    #region IDialogAware implementation

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
        var result = new DialogResult
        {
            Result = ButtonResult.Cancel
        };
        _closeDialogFromHostCommand?.Execute(result);
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        var columnProperties = parameters.GetValue<List<BaseColumnProperties>>("ColumnProperties");
        ColumnProperties = ColumnPropertiesViewModelFactory
            .FromDomainMany(columnProperties
                .OrderBy(c => c.Order));
    }

    public DialogCloseListener RequestClose { get; } = new();

    #endregion

}