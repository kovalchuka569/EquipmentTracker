using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Prism.Commands;

using JetBrains.Annotations;
using Models.Common.Table.ColumnProperties;
using Models.Equipment;

using Presentation.Attributes;
using Presentation.Enums;
using Syncfusion.Linq;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public class ListColumnPropertiesViewModel : BaseColumnPropertiesViewModel
{
    
    #region Private fields
    
    private string? _defaultValue;
    private bool _hasDefaultValue;
    private ObservableCollection<string> _listValues = [];
    private string _itemCandidate = string.Empty;
    private bool _isListValuesPopupOpen;
    
    #endregion

    #region Public fields
    
    [DisplayName(ColumnDesignerConstants.DefaultValueDisplayedUiName)]
    [Description(ColumnDesignerConstants.DefaultValuePropertyUiDescription)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Editor(EditorType.DefaultValueEditor)]
    [Order(2)]
    public override object? DefaultValue
    {
        get => _defaultValue;
        set
        {
            if (value is not (string or null))
                return;
            
            SetProperty(ref _defaultValue, (string?)value);
        }
    }

    public override bool HasDefaultValue
    {
        get => _hasDefaultValue;
        set
        {
            if(ListValues.Count is 0)
                return;
            
            if(!SetProperty(ref _hasDefaultValue, value))
                return;
            
            DefaultValue = value
                ? ListValues.First()
                : null;
        }
    }

    [DisplayName(ColumnDesignerConstants.ListValuesUiName)]
    [Description(ColumnDesignerConstants.ListValueUiDescription)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Editor(EditorType.ListEditor)]
    [Order(100)]
     public ObservableCollection<string> ListValues
     {
         get => _listValues;
         set => SetProperty(ref _listValues, value);
     }

    public string ItemCandidate
    {
        get => _itemCandidate;
        set => SetProperty(ref _itemCandidate, value);
    }

    public bool IsListValuesPopupOpen
    {
        get => _isListValuesPopupOpen;
        set => SetProperty(ref _isListValuesPopupOpen, value);
    }

    public string ShowListValuesButtonText => $"{ColumnDesignerConstants.ShowListValuesButtonText} ({ListValues.Count})";

    public bool EmptyListValuesTipVisibility => ListValues.Count == 0;
    
    #endregion
    
    #region Commands
    
    public DelegateCommand AddListValueCommand { [UsedImplicitly] get; private set; }
    public DelegateCommand OpenListPopupCommand { [UsedImplicitly] get; private set; }
    public DelegateCommand ListValuesPopupMouseLeaveCommand { [UsedImplicitly] get; private set; }
    public DelegateCommand<string> DeleteListValueCommand { [UsedImplicitly] get; private set; }
    
    #endregion

    #region Constructor
    
    public ListColumnPropertiesViewModel()
    {
        ColumnDataType = ColumnDataType.List;

        AddListValueCommand = new DelegateCommand(OnAddListValue);
        OpenListPopupCommand = new DelegateCommand(OnOpenListPopup);
        ListValuesPopupMouseLeaveCommand = new DelegateCommand(OnListValuesPopupMouseLeave);
        DeleteListValueCommand = new DelegateCommand<string>(OnDeleteListValue);
        
        ListValues.CollectionChanged += ListValues_CollectionChanged;
    }
    
    #endregion

    #region Private methods
    
    private void OnAddListValue()
    {
        ListValues.Add(ItemCandidate);
        ItemCandidate = string.Empty;
    }

    private void OnOpenListPopup()
    {
        if (IsListValuesPopupOpen)
        {
            IsListValuesPopupOpen = false;
            return;
        }
        
        IsListValuesPopupOpen = true;
    }

    private void OnListValuesPopupMouseLeave()
    {
        IsListValuesPopupOpen = false;
    }

    private void OnDeleteListValue(string listValue)
    {
        ListValues.Remove(listValue);
    }
    
    private void ListValues_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(ShowListValuesButtonText));
        RaisePropertyChanged(nameof(EmptyListValuesTipVisibility));
    }
    
    protected override void ValidateDefaultValue() { }
    
    #endregion

    #region Public methods

    public override void FromDomain(BaseColumnProperties domain)
    {
        if (domain is not ListColumnProperties listProperties)
            throw new InvalidCastException();
        
        CopyBaseFromDomain(domain);
        _hasDefaultValue = listProperties.HasDefaultValue;
        _defaultValue = (string?)listProperties.DefaultValue;
        _listValues = listProperties.ListValues.ToObservableCollection();
    }

    public override BaseColumnProperties ToDomain()
    {
        var domain = new ListColumnProperties();
        CopyBaseToDomain(domain);
        domain.HasDefaultValue = HasDefaultValue;
        domain.DefaultValue = DefaultValue;
        domain.ListValues = ListValues.ToList();
        return domain;
    }

    #endregion
    
    #region Destructor

    ~ListColumnPropertiesViewModel()
    {
        ListValues.CollectionChanged -= ListValues_CollectionChanged;
    }
    
    #endregion
}