using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Core.Services.EquipmentDataGrid;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
using Models.Equipment.ColumnSpecificSettings;
using Prism.Commands;
using Prism.Mvvm;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid.ScrollAxis;
using Brushes = System.Windows.Media.Brushes;
using ResizingColumnsEventArgs = Syncfusion.UI.Xaml.Grid.ResizingColumnsEventArgs;
using RowColumnIndex = Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex;

namespace UI.ViewModels.Equipment.DataGrid;

public class ColumnCreatorViewModel : BindableBase, INavigationAware, IDisposable, IDataErrorInfo
{
    private Action<ColumnCreationResult> _columnCreationCallback;
    private Action<ColumnEditingResult> _columnEditingCallback;
    private IRegionManager _scopedRegionManager;
    private Style _baseGridHeaderStyle;
    private readonly IEquipmentDataGridService _equipmentDataGridService;
    public Columns PreviewColumn { get; } = new();
    
    public ObservableCollection<ExpandoObject> PreviewItems { get; } = new();
    public ComboBoxConfig ComboBoxConfig { get; set; } = new();
    #region ComboBoxSelectors
    
        private ComboBoxType _selectedColumnType;
        public ComboBoxType SelectedColumnType
        {
            get => _selectedColumnType;
            set => SetProperty(ref _selectedColumnType, value);
        }
        
        private ComboBoxFontSize _selectedHeaderFontSize;
        public ComboBoxFontSize SelectedHeaderFontSize
        {
            get => _selectedHeaderFontSize;
            set => SetProperty(ref _selectedHeaderFontSize, value);
        }
    
        
        private ComboBoxFontFamily _selectedHeaderFontFamily;
        public ComboBoxFontFamily SelectedHeaderFontFamily
        {
            get => _selectedHeaderFontFamily;
            set => SetProperty(ref _selectedHeaderFontFamily, value);
        }
    
        
        private ComboBoxFontWeight _selectedHeaderFontWeight;
        public ComboBoxFontWeight SelectedHeaderFontWeight
        {
            get => _selectedHeaderFontWeight;
            set => SetProperty(ref _selectedHeaderFontWeight, value);
        }
        
        private ComboBoxVerticalAlignment _selectedHeaderVerticalAlignment;
        public ComboBoxVerticalAlignment SelectedHeaderVerticalAlignment
        {
            get => _selectedHeaderVerticalAlignment;
            set => SetProperty(ref _selectedHeaderVerticalAlignment, value);
        }

        private ComboBoxHorizontalAlignment _selectedHeaderHorizontalAlignment;
        public ComboBoxHorizontalAlignment SelectedHeaderHorizontalAlignment
        {
            get => _selectedHeaderHorizontalAlignment;
            set => SetProperty(ref _selectedHeaderHorizontalAlignment, value);
        }
        
        // Individual selectors
        
        private ComboBoxRegularExpression _selectedRegularExpression;
        public ComboBoxRegularExpression SelectedRegularExpression
        {
            get => _selectedRegularExpression;
            set => SetProperty(ref _selectedRegularExpression, value);
        }
        
        private ComboBoxDateFormat _selectedDateFormat;
        public ComboBoxDateFormat SelectedDateFormat
        {
            get => _selectedDateFormat;
            set => SetProperty(ref _selectedDateFormat, value);
        }
        
        private ComboBoxCurrency _selectedCurrency;
        public ComboBoxCurrency SelectedCurrency
        {
            get => _selectedCurrency;
            set => SetProperty(ref _selectedCurrency, value);
        }
        
    #endregion
    
    #region TextTemplate

        private long _minTextLength;
        public long MinTextLength
        {
            get => _minTextLength;
            set => SetProperty(ref _minTextLength, value);
        }
        
        private long _maxTextLength;
        public long MaxTextLength
        {
            get => _maxTextLength;
            set => SetProperty(ref _maxTextLength, value);
        }
        
    #endregion

    #region Number
    
        private int _doubleCharactersAfterComa;
        public int DoubleCharactersAfterComa
        {
            get => _doubleCharactersAfterComa;
            set => SetProperty(ref _doubleCharactersAfterComa, value);
        }
        
        private double _doubleMinValue;
        public double DoubleMinValue
        {
            get => _doubleMinValue;
            set => SetProperty(ref _doubleMinValue, value);
        }
        
        private double _doubleMaxValue;
        public double DoubleMaxValue
        {
            get => _doubleMaxValue;
            set => SetProperty(ref _doubleMaxValue, value);
        }

    #endregion

    #region BooleanTemplate

        private bool _booleanIsChecked;
        public bool BooleanIsChecked
        {
            get => _booleanIsChecked;
            set => SetProperty(ref _booleanIsChecked, value);
        }

    #endregion

    #region MultilineTemplate

    private long _maxMultilineTextLength;
    public long MaxMultilineTextLength
    {
        get => _maxMultilineTextLength;
        set => SetProperty(ref _maxMultilineTextLength, value);
    }

    #endregion
    
    #region CurrencyTemplate
    
        private bool _currencySymbolAtFirst;
        public bool CurrencySymbolAtFirst
        {
            get => _currencySymbolAtFirst;
            set => SetProperty(ref _currencySymbolAtFirst, value);
        }
        
        private bool _currencySymbolAtLast;
        public bool CurrencySymbolAtLast
        {
            get => _currencySymbolAtLast;
            set => SetProperty(ref _currencySymbolAtLast, value);
        }
        
    #endregion
    
    #region CheckBoxes

    private bool _isRequired;
    public bool IsRequired
    {
        get => _isRequired;
        set => SetProperty(ref _isRequired, value);
    }
    
    private bool _isUnique;
    public bool IsUnique
    {
        get => _isUnique;
        set => SetProperty(ref _isUnique, value);
    }
    
    private bool _isReadOnly;
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    
    private bool _allowSorting;
    public bool AllowSorting
    {
        get => _allowSorting;
        set => SetProperty(ref _allowSorting, value);
    }
    
    private bool _allowFiltering;
    public bool AllowFiltering
    {
        get => _allowFiltering;
        set => SetProperty(ref _allowFiltering, value);
    }
    
    private bool _allowGrouping;
    public bool AllowGrouping
    {
        get => _allowGrouping;
        set => SetProperty(ref _allowGrouping, value);
    }
    
    private bool _isPinned;
    public bool IsPinned
    {
        get => _isPinned;
        set => SetProperty(ref _isPinned, value);
    }
    
    #endregion
    
    private string _headerText;
    public string HeaderText
    {
        get => _headerText;
        set => SetProperty(ref _headerText, value);
    }
    
    #region Colors

    private Color _headerBackgroundColor;
    public Color HeaderBackgroundColor
    {
        get => _headerBackgroundColor;
        set => SetProperty(ref _headerBackgroundColor, value);
    }
    
    private Color _headerForegroundColor;
    public Color HeaderForegroundColor
    {
        get => _headerForegroundColor;
        set => SetProperty(ref _headerForegroundColor, value);
    }
    
    private Color _headerBorderColor;
    public Color HeaderBorderColor
    {
        get => _headerBorderColor;
        set => SetProperty(ref _headerBorderColor, value);
    }
    #endregion
    
    private double _columnWidth;

    public ObservableCollection<string> ListValues { get; set; } = new();
    
    private string _newValue;
    public string NewValue
    {
        get => _newValue;
        set => SetProperty(ref _newValue, value);
    }

    private bool _nullListValueVisibility = true;
    public bool NullListValueVisibility
    {
        get => _nullListValueVisibility;
        set => SetProperty(ref _nullListValueVisibility, value);
    }

    private double _topBorderThickness;
    public double TopBorderThickness
    {
        get => _topBorderThickness;
        set => SetProperty(ref _topBorderThickness, value);
    }
    
    private double _leftBorderThickness;
    public double LeftBorderThickness
    {
        get => _leftBorderThickness;
        set => SetProperty(ref _leftBorderThickness, value);
    }
    
    private double _rightBorderThickness;
    public double RightBorderThickness
    {
        get => _rightBorderThickness;
        set => SetProperty(ref _rightBorderThickness, value);
    }
    
    private double _bottomBorderThickness;
    public double BottomBorderThickness
    {
        get => _bottomBorderThickness;
        set => SetProperty(ref _bottomBorderThickness, value);
    }

    private string _errorContent;
    public string ErrorContent
    {
        get => _errorContent;
        set => SetProperty(ref _errorContent, value);
    }

    private bool _isDataTypeComboBoxEnabled = true;
    public bool IsDataTypeComboBoxEnabled
    {
        get => _isDataTypeComboBoxEnabled;
        set => SetProperty(ref _isDataTypeComboBoxEnabled, value);
    }
    
    private bool _isEditing;
    private ColumnItem _editingColumn;
    private int _editingColumnId;
    private string _mappingName;
    
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["ColumnCreatedCallback"] is Action<ColumnCreationResult> callback)
        {   
            _isEditing = false;
            _columnCreationCallback = callback;
            LoadDefaultProperties();
        }
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {   
            _scopedRegionManager = scopedRegionManager;
        }
        if (navigationContext.Parameters["BaseHeaderStyle"] is Style baseHeaderStyle)
        {   
            _baseGridHeaderStyle = baseHeaderStyle;
        }
        if (navigationContext.Parameters["EditingColumnItem"] is ColumnItem editingColumnItem)
        {
            _editingColumn = editingColumnItem;
        }
        if (navigationContext.Parameters["ColumnEditingCallback"] is Action<ColumnEditingResult> editingCallback)
        {   
            _isEditing = true;
            _columnEditingCallback = editingCallback;
            LoadEditingProperties();
        }
        
        ListValues.CollectionChanged += (sender, args) =>
        {
            NullListValueVisibility = ListValues.Count == 0;
        };
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public DelegateCommand CloseColumnCreatorCommand { get; }
    public DelegateCommand AddNewValueCommand { get; }
    public DelegateCommand<string> RemoveValueCommand { get; }
    public DelegateCommand UpdatePreviewCommand { get; }
    public DelegateCommand SaveColumnCommand { get; }
    public ColumnCreatorViewModel(IEquipmentDataGridService equipmentDataGridService)
    {
        _equipmentDataGridService = equipmentDataGridService;
        
        CloseColumnCreatorCommand = new DelegateCommand(OnCloseColumnCreator);
        AddNewValueCommand = new DelegateCommand(OnAddNewValue);
        RemoveValueCommand= new DelegateCommand<string> (OnRemoveValue);
        UpdatePreviewCommand = new DelegateCommand(LoadPreview);
        SaveColumnCommand = new DelegateCommand(OnSaveColumn);
    }

    private SfDataGrid _sfDataGrid;
    
    private void OnResizingColumn(object? sender, ResizingColumnsEventArgs e)
    {
        _columnWidth = e.Width;
    }

    private void OnSaveColumn()
    {
        if (_isEditing)
        {
            EditingColumn();
        }
        else if (!_isEditing)
        {
            CreatingColumn();
        }
    }

    private async void CreatingColumn()
    {
        var columnSettings = new ColumnSettings
        {
            DataType = SelectedColumnType.ColumnDataType,

            HeaderText = HeaderText,
            MappingName = _mappingName,

            // Colors
            HeaderBackground = HeaderBackgroundColor,
            HeaderForeground = HeaderForegroundColor,
            HeaderBorderColor = HeaderBorderColor,

            // Font
            HeaderFontFamily = SelectedHeaderFontFamily.FontFamily,
            HeaderFontSize = SelectedHeaderFontSize.FontSize,
            HeaderFontWeight = SelectedHeaderFontWeight.FontWeight,

            // Alignments
            HeaderHorizontalAlignment = SelectedHeaderHorizontalAlignment.Alignment,
            HeaderVerticalAlignment = SelectedHeaderVerticalAlignment.Alignment,

            // Size
            ColumnWidth = _columnWidth,

            // CheckBoxes
            IsReadOnly = IsReadOnly,
            IsUnique = IsUnique,
            IsRequired = IsRequired,
            AllowSorting = AllowSorting,
            AllowFiltering = AllowFiltering,
            AllowGrouping = AllowGrouping,
            IsPinned = IsPinned,

            // Border Thickness
            HeaderBorderThickness = new Thickness(LeftBorderThickness, TopBorderThickness, RightBorderThickness,
                BottomBorderThickness),

            SpecificSettings = CreateSpecificSettings(SelectedColumnType.ColumnDataType),
        };
        _columnCreationCallback.Invoke(new ColumnCreationResult{ IsSuccessful = true, ColumnSettings = columnSettings });
    }

    private void EditingColumn()
    {
        var s = _editingColumn.Settings;
        s.HeaderText = HeaderText;

        s.HeaderBackground = HeaderBackgroundColor;
        s.HeaderForeground = HeaderForegroundColor;
        s.HeaderBorderColor = HeaderBorderColor;

        s.HeaderFontFamily = SelectedHeaderFontFamily.FontFamily;
        s.HeaderFontSize = SelectedHeaderFontSize.FontSize;
        s.HeaderFontWeight = SelectedHeaderFontWeight.FontWeight;

        s.HeaderHorizontalAlignment = SelectedHeaderHorizontalAlignment.Alignment;
        s.HeaderVerticalAlignment = SelectedHeaderVerticalAlignment.Alignment;

        s.ColumnWidth = _columnWidth;

        s.IsReadOnly = IsReadOnly;
        s.IsUnique = IsUnique;
        s.IsRequired = IsRequired;
        s.AllowSorting = AllowSorting;
        s.AllowFiltering = AllowFiltering;
        s.AllowGrouping = AllowGrouping;
        s.IsPinned = IsPinned;

        s.HeaderBorderThickness = new Thickness(LeftBorderThickness, TopBorderThickness, RightBorderThickness, BottomBorderThickness);
        s.SpecificSettings = CreateSpecificSettings(SelectedColumnType.ColumnDataType);

        _columnEditingCallback.Invoke(new ColumnEditingResult { IsSuccessful = true, Column = _editingColumn });
    }

    private object CreateSpecificSettings(ColumnDataType dataType)
    {
        return dataType switch
        {
            ColumnDataType.Text => new TextColumnSettings
            {
                RegularExpressionPattern = SelectedRegularExpression.RegularExpressionPattern,
                MaxLength = MaxTextLength,
                MinLength = MinTextLength,
            },
            ColumnDataType.Number => new NumberColumnSettings
            {
                CharactersAfterComma = DoubleCharactersAfterComa,
                MaxValue = DoubleMaxValue,
                MinValue = DoubleMinValue,
            },
            ColumnDataType.Boolean => new BooleanColumnSettings
            {
                DefaultValue = BooleanIsChecked,
            },
            ColumnDataType.Date => new DateColumnSettings
            {
                DateFormat = SelectedDateFormat.Format,
            },
            ColumnDataType.Currency => new CurrencyColumnSettings
            {
                CurrencySymbol = SelectedCurrency.Currency,
                PositionBefore = CurrencySymbolAtFirst,
                PositionAfter = CurrencySymbolAtLast,
            },
            ColumnDataType.List => new ListColumnSettings
            {
                ListValues = ListValues,
            },
            ColumnDataType.MultilineText => new MultilineTextColumnSettings
            {
                MaxLength = MaxMultilineTextLength,
            },
            ColumnDataType.Hyperlink => null
        };
    }

    private void LoadSpecificProperties(ColumnDataType dataType, object settings)
    {
        switch (dataType)
        {
            case ColumnDataType.Text:
                if (settings is TextColumnSettings textColumnSettings)
                {
                    SelectedRegularExpression = ComboBoxConfig.RegularExpressions.FirstOrDefault(x => x.RegularExpressionPattern == textColumnSettings.RegularExpressionPattern);
                    MinTextLength = textColumnSettings.MinLength;
                    MaxTextLength = textColumnSettings.MaxLength;
                }
                break;
            case ColumnDataType.Number:
                if (settings is NumberColumnSettings numberColumnSettings)
                {
                    DoubleCharactersAfterComa = numberColumnSettings.CharactersAfterComma;
                    DoubleMaxValue = numberColumnSettings.MaxValue;
                    DoubleMinValue = numberColumnSettings.MinValue;
                }
                break;
            case ColumnDataType.Boolean:
                if (settings is BooleanColumnSettings booleanColumnSettings)
                {
                    BooleanIsChecked = booleanColumnSettings.DefaultValue;
                };
                break;
            case ColumnDataType.Date:
                if (settings is DateColumnSettings dateColumnSettings)
                {
                    SelectedDateFormat = ComboBoxConfig.DateFormats.FirstOrDefault(x => x.Format == dateColumnSettings.DateFormat);
                }
                break;
            case ColumnDataType.Currency:
                if (settings is CurrencyColumnSettings currencyColumnSettings)
                {
                    SelectedCurrency = ComboBoxConfig.Currencies.FirstOrDefault(x => x.Currency == currencyColumnSettings.CurrencySymbol);
                    CurrencySymbolAtFirst = currencyColumnSettings.PositionBefore;
                    CurrencySymbolAtLast = currencyColumnSettings.PositionAfter;
                }
                break;
            case ColumnDataType.List:
                if (settings is ListColumnSettings listColumnSettings)
                {
                    ListValues = listColumnSettings.ListValues;
                    NullListValueVisibility = ListValues.Count == 0;
                }
                break;
            case ColumnDataType.MultilineText:
                if (settings is MultilineTextColumnSettings multilineTextColumnSettings)
                {
                    MaxMultilineTextLength = multilineTextColumnSettings.MaxLength;
                }
                break;
        }
    }
    
    private void LoadEditingProperties()
    {
        IsDataTypeComboBoxEnabled = !_isEditing;
        var s = _editingColumn.Settings;
        
        SelectedColumnType = ComboBoxConfig.ColumnTypes.FirstOrDefault(x => x.ColumnDataType == s.DataType);
        
        HeaderText = s.HeaderText;
        _mappingName = s.MappingName;
        
        SelectedHeaderFontFamily = ComboBoxConfig.FontFamilies.FirstOrDefault(x => x.FontFamily == s.HeaderFontFamily);
        SelectedHeaderFontSize = ComboBoxConfig.FontSizes.FirstOrDefault(x => x.FontSize == s.HeaderFontSize);
        SelectedHeaderFontWeight = ComboBoxConfig.FontWeights.FirstOrDefault(x => x.FontWeight == s.HeaderFontWeight);
        
        SelectedHeaderHorizontalAlignment = ComboBoxConfig.HorizontalAlignments.FirstOrDefault(x => x.Alignment == s.HeaderHorizontalAlignment);
        SelectedHeaderVerticalAlignment = ComboBoxConfig.VerticalAlignments.FirstOrDefault(x => x.Alignment == s.HeaderVerticalAlignment);
        
        IsReadOnly = s.IsReadOnly;
        IsUnique = s.IsUnique;
        IsRequired = s.IsRequired;
        AllowSorting = s.AllowSorting;
        AllowFiltering = s.AllowFiltering;
        AllowGrouping = s.AllowGrouping;
        IsPinned = s.IsPinned;
        
        HeaderBorderColor = s.HeaderBorderColor;
        HeaderBackgroundColor = s.HeaderBackground;
        HeaderForegroundColor = s.HeaderForeground;
        
        LeftBorderThickness = s.HeaderBorderThickness.Left;
        RightBorderThickness = s.HeaderBorderThickness.Right;
        TopBorderThickness = s.HeaderBorderThickness.Top;
        BottomBorderThickness = s.HeaderBorderThickness.Bottom;

        _columnWidth = s.ColumnWidth;
        
        LoadSpecificProperties(s.DataType, s.SpecificSettings);
        LoadPreview();
    }

    private void LoadDefaultProperties()
    {
        SelectedColumnType = ComboBoxConfig.ColumnTypes.FirstOrDefault(x => x.ColumnDataType == ColumnDataType.Text);
        HeaderText = "Назва заголовку";
        _mappingName = Guid.NewGuid().ToString("N");
        
        // Text
        SelectedRegularExpression = ComboBoxConfig.RegularExpressions.First();
        MinTextLength = 0;
        MaxTextLength = 100;
        
        // Number
        DoubleCharactersAfterComa = 2;
        DoubleMinValue = 0;
        DoubleMaxValue = 100;
        
        // Date
        SelectedDateFormat = ComboBoxConfig.DateFormats.First();
        
        // Boolean
        BooleanIsChecked = false;
        
        // Multiline
        MaxMultilineTextLength = 500;
        
        // Currency
        CurrencySymbolAtLast = true;
        
        // CheckBoxes
        IsRequired = false;
        IsUnique = false;
        IsReadOnly = false;
        AllowSorting = true;
        AllowFiltering = true;
        AllowGrouping = true;
        IsPinned = false;
        
        // Colors
        HeaderBackgroundColor = Colors.LightGray;
        HeaderForegroundColor = Colors.Black;
        HeaderBorderColor = Colors.Gray;
        
        // Font 
        SelectedHeaderFontFamily = ComboBoxConfig.FontFamilies.FirstOrDefault(x => x.FontFamily == "Segoe UI");
        SelectedHeaderFontSize = ComboBoxConfig.FontSizes.FirstOrDefault(x => x.FontSize == 14);
        SelectedHeaderFontWeight = ComboBoxConfig.FontWeights.FirstOrDefault(x => x.FontWeight == FontWeights.Thin);
        
        // Alignment
        SelectedHeaderHorizontalAlignment = ComboBoxConfig.HorizontalAlignments.FirstOrDefault(x => x.Alignment == HorizontalAlignment.Center);
        SelectedHeaderVerticalAlignment = ComboBoxConfig.VerticalAlignments.FirstOrDefault(x => x.Alignment == VerticalAlignment.Center);

        _columnWidth = 250;
        
        // Border
        LeftBorderThickness = 1;
        TopBorderThickness = 1;
        RightBorderThickness = 1;
        BottomBorderThickness = 1;
        
        // Currency
        SelectedCurrency = ComboBoxConfig.Currencies.First(x => x.Currency == "");
        LoadPreview();
    }
    
    private readonly Dictionary<object, double> RowHeights = new();
    private double DefaultHeight = 30;
    private double DefaultHeaderHeight = 30;
    private double CustomHeaderHeight = 30;
    private void SfDataGrid_OnQueryRowHeight(object? sender, QueryRowHeightEventArgs e)
        {
            var dataGrid = (SfDataGrid)sender!;
            
            if (e.RowIndex > 0)
            {
                var rowData = dataGrid.GetRecordAtRowIndex(e.RowIndex);
                if (rowData != null && RowHeights.TryGetValue(rowData, out double height))
                {
                    e.Height = height;
                    e.Handled = true;
                }
                else
                {
                    e.Height = DefaultHeight;
                    e.Handled = true;
                }
            }
        }

    private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (e.Delta > 0)
                {
                    ChangeHeaderHeight(+10);
                }
                else if (e.Delta < 0)
                {
                    ChangeHeaderHeight(-10);
                }
                e.Handled = true;
            }
            else
            {
                var item = _sfDataGrid.SelectedItem;
                if (item == null) return;

                if (e.Delta > 0)
                {
                    ChangeRowHeight(item, +10);
                }
                else if (e.Delta < 0)
                {
                    ChangeRowHeight(item, -10);
                }
                e.Handled = true;
            }
        }
    }

    private void ChangeHeaderHeight(double delta)
    {
        CustomHeaderHeight = Math.Max(20, CustomHeaderHeight + delta);
        _sfDataGrid.HeaderRowHeight = CustomHeaderHeight;
        _sfDataGrid.InvalidateMeasure();
        _sfDataGrid.InvalidateArrange();
        _sfDataGrid.InvalidateVisual();
        _sfDataGrid.UpdateLayout(); 
    }

    private void ChangeRowHeight(object item, double delta)
    {
        if (!RowHeights.TryGetValue(item, out double currentHeight))
            currentHeight = DefaultHeight;

        double newHeight = Math.Max(20, currentHeight + delta);
        RowHeights[item] = newHeight;

        int rowIndex = _sfDataGrid.ResolveToRowIndex(item);
        var rowColumnIndex = new RowColumnIndex(rowIndex, 0);
        _sfDataGrid.InvalidateRowHeight(rowIndex);
        _sfDataGrid.ScrollInView(rowColumnIndex);
        _sfDataGrid.InvalidateMeasure(); 
        _sfDataGrid.InvalidateArrange();
        _sfDataGrid.InvalidateVisual();
        _sfDataGrid.UpdateLayout();
    }
    
    private UIElement _dataGridContent;
    public UIElement DataGridContent
    {
        get => _dataGridContent;
        set => SetProperty(ref _dataGridContent, value);
    }
    
    private static SolidColorBrush ConvertFromColor(Color color)
    {
        return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
    }
    
    public void ReloadDataGrid()
    { 
        if (_sfDataGrid != null)
        {
            _sfDataGrid.QueryRowHeight -= SfDataGrid_OnQueryRowHeight;
            _sfDataGrid.PreviewMouseWheel -= UIElement_OnPreviewMouseWheel;
            _sfDataGrid.ResizingColumns -= OnResizingColumn;
        }
        
        var newGrid = new SfDataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = PreviewItems,
            Columns = PreviewColumn,
            AllowResizingColumns = true,
            IsDynamicItemsSource = true,
            HeaderRowHeight = CustomHeaderHeight,
            BorderThickness = new Thickness(0),
            AllowResizingHiddenColumns = true,
        };
        newGrid.QueryRowHeight += SfDataGrid_OnQueryRowHeight;
        newGrid.PreviewMouseWheel += UIElement_OnPreviewMouseWheel;
        newGrid.ResizingColumns += OnResizingColumn;
        _sfDataGrid = newGrid;
        DataGridContent = newGrid;
    }
    private void LoadPreview()
    {
        string oldHeaderText = PreviewColumn.FirstOrDefault()?.MappingName;
        if (string.IsNullOrWhiteSpace(HeaderText))
        {
            return;
        }
        if (!string.IsNullOrEmpty(oldHeaderText) && oldHeaderText != HeaderText)
        {
            foreach (var item in PreviewItems)
            {
                var dict = item as IDictionary<string, object>;
                if (dict != null)
                {
                    if (dict.ContainsKey(oldHeaderText))
                    {
                        var value = dict[oldHeaderText];
                        dict.Remove(oldHeaderText);
                        dict[HeaderText] = value;
                    }
                    else if (!dict.ContainsKey(HeaderText))
                    {
                        dict[HeaderText] = null;
                    }
                }
            }
        }
        
        PreviewColumn.Clear();
        
        var fontFamilyConverter = new FontFamilyConverter();
        
        var headerTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
        headerTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding());
        headerTextBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        var dataTemplate = new DataTemplate
        {
            VisualTree = headerTextBlockFactory
        };
        var headerStyle = new Style(typeof(GridHeaderCellControl), _baseGridHeaderStyle);
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.BorderThicknessProperty, new Thickness(LeftBorderThickness, TopBorderThickness, RightBorderThickness, BottomBorderThickness)));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.FontSizeProperty, SelectedHeaderFontSize.FontSize));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.FontWeightProperty, SelectedHeaderFontWeight.FontWeight));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.FontFamilyProperty,fontFamilyConverter.ConvertFromString(SelectedHeaderFontFamily.FontFamily)));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.ForegroundProperty, ConvertFromColor(HeaderForegroundColor)));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.BorderBrushProperty, ConvertFromColor(HeaderBorderColor)));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.BackgroundProperty, ConvertFromColor(HeaderBackgroundColor)));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.VerticalContentAlignmentProperty, SelectedHeaderVerticalAlignment.Alignment));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.HorizontalContentAlignmentProperty, SelectedHeaderHorizontalAlignment.Alignment));
        headerStyle.Setters.Add(new Setter(GridHeaderCellControl.ContentTemplateProperty, dataTemplate));
        
        switch (SelectedColumnType.ColumnDataType)
        {
            case ColumnDataType.Text:
                PreviewColumn.Add(new GridTextColumn
                {
                    HeaderText = HeaderText, 
                    MappingName = _mappingName, 
                    HeaderStyle = headerStyle,
                    AllowSorting = AllowSorting,
                    AllowGrouping = AllowGrouping,
                    AllowFiltering = AllowFiltering,
                    Width = _columnWidth
                });
                ReloadDataGrid();
                break;
            case ColumnDataType.Date:
                PreviewColumn.Add(new GridDateTimeColumn 
                { 
                    HeaderText = HeaderText, 
                    MappingName = _mappingName, 
                    HeaderStyle = headerStyle, 
                    AllowSorting = AllowSorting,
                    AllowGrouping = AllowGrouping,
                    AllowFiltering = AllowFiltering,
                    Width = _columnWidth,
                });
                ReloadDataGrid();
                break;
            case ColumnDataType.Number:
                PreviewColumn.Add(new GridNumericColumn                
                { 
                    HeaderText = HeaderText, 
                    MappingName = _mappingName, 
                    HeaderStyle = headerStyle, 
                    AllowSorting = AllowSorting,
                    AllowGrouping = AllowGrouping,
                    AllowFiltering = AllowFiltering,
                    Width = _columnWidth,
                });
                ReloadDataGrid();
                break;
            case ColumnDataType.Boolean:
                PreviewColumn.Add(new GridCheckBoxColumn                
                { 
                    HeaderText = HeaderText, 
                    MappingName = _mappingName, 
                    HeaderStyle = headerStyle, 
                    AllowSorting = AllowSorting,
                    AllowGrouping = AllowGrouping,
                    AllowFiltering = AllowFiltering,
                    Width = _columnWidth,
                });
                ReloadDataGrid();
                break;
            case ColumnDataType.Currency:
                PreviewColumn.Add(new GridCurrencyColumn                
                { 
                    HeaderText = HeaderText, 
                    MappingName = _mappingName, 
                    HeaderStyle = headerStyle, 
                    AllowSorting = AllowSorting,
                    AllowGrouping = AllowGrouping,
                    AllowFiltering = AllowFiltering,
                    Width = _columnWidth,
                });
                ReloadDataGrid();
                break;
            case ColumnDataType.Hyperlink:
                PreviewColumn.Add(new GridTextColumn                
                { 
                    HeaderText = HeaderText, 
                    MappingName = _mappingName, 
                    HeaderStyle = headerStyle, 
                    AllowSorting = AllowSorting,
                    AllowGrouping = AllowGrouping,
                    AllowFiltering = AllowFiltering,
                    Width = _columnWidth,
                });
                ReloadDataGrid();
                break;
            case ColumnDataType.List:
                PreviewColumn.Add(new GridComboBoxColumn               
                { 
                    HeaderText = HeaderText, 
                    MappingName = _mappingName, 
                    HeaderStyle = headerStyle, 
                    AllowSorting = AllowSorting,
                    AllowGrouping = AllowGrouping,
                    AllowFiltering = AllowFiltering,
                    Width = _columnWidth,
                });
                ReloadDataGrid();
                break;
            case ColumnDataType.MultilineText:
                PreviewColumn.Add(new GridTextColumn                 
                { 
                    HeaderText = HeaderText, 
                    MappingName = _mappingName, 
                    HeaderStyle = headerStyle, 
                    AllowSorting = AllowSorting,
                    AllowGrouping = AllowGrouping,
                    AllowFiltering = AllowFiltering,
                    Width = _columnWidth,
                });
                ReloadDataGrid();
                break;
        }
    }

    private void OnAddNewValue()
    {
        if (!string.IsNullOrWhiteSpace(NewValue))
        {
            ListValues.Add(NewValue);
            NewValue = string.Empty;
        }
    }

    private void OnRemoveValue(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && ListValues.Contains(value))
        {
            ListValues.Remove(value);
        }
    }

    private void OnCloseColumnCreator()
    {
        if (!_isEditing)
        {
            _columnCreationCallback.Invoke(new ColumnCreationResult{IsSuccessful = false});
        }
        else if(_isEditing)
        {
            _columnEditingCallback.Invoke(new ColumnEditingResult{IsSuccessful = false});
        }
    }


    public void Dispose()
    {
        Console.WriteLine("Dispose");
    }

    private string _error;
    public string Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    private Dictionary<string, string> _errors = new();

    public string this[string columnName]
    {
        get
        {
            string error = OnValidate(columnName);
            if (string.IsNullOrEmpty(error))
            {
                _errors.Remove(columnName);
            }
            else
            {
                _errors[columnName] = error;
            }

            Error = _errors.Any() ? "Виправіть помилки" : null;
            Error = error;
            
            RaisePropertyChanged(nameof(ErrorCount));
            RaisePropertyChanged(nameof(IsValid));
            
            return error;
        }
    }
    
    public int ErrorCount => _errors.Count;

    public bool IsValid => ErrorCount == 0;

    private string OnValidate(string columnName)
    {
        string result = string.Empty;
        if (columnName == "HeaderText")
        {
            if (string.IsNullOrWhiteSpace(HeaderText))
            {
                result = "Заголовок не може бути порожнім";
            }
            if (HeaderText.Length > 60)
            {
                result = "Максимальна довжина заголовку - 60 символів";
            }
        }
        if (columnName == "MaxTextLength")
        {
            if (MaxTextLength < MinTextLength)
            {
                result = "Максимальне значення повинно бути більшим за мінімальне";
            }
        }
        if (columnName == "MinTextLength")
        {
            if (MaxTextLength < MinTextLength)
            {
                result = "Максимальне значення повинно бути більшим за мінімальне";
            }
        }
        if (columnName == "DoubleMinValue")
        {
            if (DoubleMaxValue < DoubleMinValue)
            {
                result = "Максимальне значення повинно бути більшим за мінімальне";
            }
        }
        if (columnName == "DoubleMaxValue")
        {
            if (DoubleMaxValue < DoubleMinValue)
            {
                result = "Максимальне значення повинно бути більшим за мінімальне";
            }
        }
        return result;
    }
}