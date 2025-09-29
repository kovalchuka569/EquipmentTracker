using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Common.Extensions;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Tools.Controls;

namespace Common.Behaviors;

public class SfDataGridToolBarBehavior : Behavior<FrameworkElement>
{
    #region Dependency properties
    
    public static readonly DependencyProperty SearchHelperProperty = DependencyProperty.Register(nameof(SearchHelper), typeof(SearchHelper), typeof(SfDataGridToolBarBehavior), new PropertyMetadata(OnSearchHelperChanged));
    public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(SfDataGridToolBarBehavior));
    public static readonly DependencyProperty PrintCommandProperty = DependencyProperty.Register(nameof(PrintCommand), typeof(ICommand), typeof(SfDataGridToolBarBehavior));
    public static readonly DependencyProperty ImportFromExcelCommandProperty = DependencyProperty.Register(nameof(ImportFromExcelCommand), typeof(ICommand), typeof(SfDataGridToolBarBehavior));
    public static readonly DependencyProperty ExportToExcelCommandProperty = DependencyProperty.Register(nameof(ExportToExcelCommand), typeof(ICommand), typeof(SfDataGridToolBarBehavior));
    public static readonly DependencyProperty ExportToPdfCommandProperty = DependencyProperty.Register(nameof(ExportToPdfCommand), typeof(ICommand), typeof(SfDataGridToolBarBehavior));
    public static readonly DependencyProperty ColumnSettingsCommandProperty = DependencyProperty.Register(nameof(ColumnSettingsCommand), typeof(ICommand), typeof(SfDataGridToolBarBehavior));

    #endregion
    
    #region Constants
    
    private const string RefreshButtonName = "PART_RefreshButton";
    private const string PrintButtonName = "PART_PrintButton";
    private const string ImportDropDownButtonAdvName = "PART_ImportDropDownButtonAdv";
    private const string ExportDropDownButtonAdvName = "PART_ExportDropDownButtonAdv";
    private const string DropDownButtonPopupName = "PART_DropDown";
    private const string ImportFromExcelDropDownMenuItemName = "PART_ImportFromExcelDropDownMenuItem";
    private const string ExportToExcelDropDownMenuItemName = "PART_ExportToExcelDropDownMenuItem";
    private const string ExportToPdfDropDownMenuItemName = "PART_ExportToPdfDropDownMenuItem";
    private const string SearchPopupName = "PART_SearchPopup";
    private const string SearchTypeDropDownButtonAdvName = "PART_SearchTypeDropDownButtonAdv";
    private const string StartWithDropDownItemName = "PART_StartWithDropDownItem";
    private const string ContainsDropDownItemName = "PART_ContainsDropDownItem";
    private const string EndWithDropDownItemName = "PART_EndWithDropDownItem";
    private const string StartWithRadioButtonName = "PART_StartWithRadioButton";
    private const string ContainsRadioButtonName = "PART_ContainsRadioButton";
    private const string EndWithRadioButtonName = "PART_EndWithRadioButton";
    private const string SearchTextBoxName = "PART_SearchTextBox";
    private const string PreviousSearchResultButtonName = "PART_PreviousSearchResultButton";
    private const string NextSearchResultButtonName = "PART_NextSearchResultButton";
    private const string ClearSearchButtonName = "PART_ClearSearchButton";
    private const string ColumnSettingsButtonName = "PART_ColumnSettingsButton";
    private const SearchType DefaultSearchType = SearchType.Contains;
    private const int DebounceMsInterval = 500;
    
    #endregion
    
    #region Private Fields
    
    private DispatcherTimer? _searchDebounceTimer;
    private DropDownButtonAdv? _importDropDownButtonAdv;
    private DropDownButtonAdv? _exportDropDownButtonAdv;
    private Popup? _searchPopup;
    private DropDownButtonAdv? _searchTypeDropDownButtonAdv;
    private RadioButton? _startWithRadioButton;
    private RadioButton? _containsRadioButton;
    private RadioButton? _endWithRadioButton;
    private SearchType _searchType = DefaultSearchType;
    private TextBox? _searchTextBox;
    private string _searchText = string.Empty;
    private ButtonBase? _previousSearchResultButton;
    private ButtonBase? _nextSearchResultButton;
    private ButtonBase? _clearSearchButton;
    
    #endregion
    
    #region Public fields
    
    public SearchHelper? SearchHelper
    {
        get => (SearchHelper?)GetValue(SearchHelperProperty);
        set => SetValue(SearchHelperProperty, value);
    }

    public ICommand? RefreshCommand
    {
        get => (ICommand?)GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    public ICommand? PrintCommand
    {
        get => (ICommand?)GetValue(PrintCommandProperty);
        set => SetValue(PrintCommandProperty, value);
    }

    public ICommand? ImportFromExcelCommand
    {
        get => (ICommand?)GetValue(ImportFromExcelCommandProperty);
        set => SetValue(ImportFromExcelCommandProperty, value);
    }

    public ICommand? ExportToExcelCommand
    {
        get => (ICommand?)GetValue(ExportToExcelCommandProperty);
        set => SetValue(ExportToExcelCommandProperty, value);
    }

    public ICommand? ExportToPdfCommand
    {
        get => (ICommand?)GetValue(ExportToPdfCommandProperty);
        set => SetValue(ExportToPdfCommandProperty, value);
    }

    public ICommand? ColumnSettingsCommand
    {
        get => (ICommand?)GetValue(ColumnSettingsCommandProperty);
        set => SetValue(ColumnSettingsCommandProperty, value);
    }
    
    #endregion
    
    #region Private methods

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += AssociatedObject_OnLoaded;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_OnLoaded;

        if (_exportDropDownButtonAdv != null)
        {
            _exportDropDownButtonAdv.DropDownOpened -= ExportDropDownButton_DropDownOpened;
            _exportDropDownButtonAdv.Dispose();
            _exportDropDownButtonAdv = null;
        }

        if (_importDropDownButtonAdv != null)
        {
            _importDropDownButtonAdv.DropDownOpened -= ExportDropDownButton_DropDownOpened;
            _importDropDownButtonAdv.Dispose();
            _importDropDownButtonAdv = null;
        }
        
        if (_searchPopup != null)
        {
            _searchPopup.Opened -= SearchPopup_OnOpened;
            _searchPopup = null;
        }

        if (_searchTextBox != null)
        {
            _searchTextBox.TextChanged -= SearchTextBox_OnTextChanged;
            _searchTextBox = null;
        }

        if (_previousSearchResultButton != null)
        {
            _previousSearchResultButton.Click -= PreviousSearchResultButton_OnClick;
            _previousSearchResultButton = null;
        }

        if (_nextSearchResultButton != null)
        {
            _nextSearchResultButton.Click -= NextSearchResultButton_OnClick;
            _nextSearchResultButton = null;
        }

        if (_clearSearchButton != null)
        {
            _clearSearchButton.Click -= ClearSearchButton_OnClick;
            _clearSearchButton = null;
        }
        
        base.OnDetaching();
    }
    
    private static void OnSearchHelperChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(e.NewValue is not SearchHelper newHelper)
            return;
        
        newHelper.AllowFiltering = true;
        newHelper.SearchType = DefaultSearchType;
    }
    
    private void AttachImportDropDownButton(DropDownButtonAdv dropDownButtonAdv)
    {
        if(_importDropDownButtonAdv != null)
            _importDropDownButtonAdv.DropDownOpened -= ImportDropDownButton_DropDownOpened;
        
        _importDropDownButtonAdv = dropDownButtonAdv;
        _importDropDownButtonAdv.DropDownOpened += ImportDropDownButton_DropDownOpened;
    }
    
    private void AttachExportDropDownButton(DropDownButtonAdv dropDownButtonAdv)
    {
        if(_exportDropDownButtonAdv != null)
            _exportDropDownButtonAdv.DropDownOpened -= ExportDropDownButton_DropDownOpened;
        
        _exportDropDownButtonAdv = dropDownButtonAdv;
        _exportDropDownButtonAdv.DropDownOpened += ExportDropDownButton_DropDownOpened;
    }

    private void AttachToSearchPopup(Popup popup)
    {
        if(_searchPopup != null)
            _searchPopup.Opened -= SearchPopup_OnOpened;
        
        _searchPopup = popup;
        _searchPopup.Opened += SearchPopup_OnOpened;
    }

    private void AttachToColumnSettingsButton(ButtonBase button)
    {
        button.Command = ColumnSettingsCommand;
    }

    private void SearchTypeChanged(SearchType newSearchType)
    {
        _searchType = newSearchType;
        Console.WriteLine(_searchType);
        ClearSearch();
        if (SearchHelper != null) 
            SearchHelper.SearchType = _searchType;
    }

    private void ClearSearch()
    {
        if (_searchTextBox != null)
        {
            _searchTextBox.Text = string.Empty;
        }
        SearchHelper?.ClearSearch();
    }
    
    #endregion

    #region Event handlers
    
    private void AssociatedObject_OnLoaded(object sender, RoutedEventArgs e)
    {
        var newRefreshButton = VisualTreeHelperEx
            .FindVisualChildren<ButtonBase>(AssociatedObject)
            .FirstOrDefault(p => p.Name == RefreshButtonName);
        if (newRefreshButton != null)
            newRefreshButton.Command = RefreshCommand;
        
        var newPrintButton = VisualTreeHelperEx
            .FindVisualChildren<ButtonBase>(AssociatedObject)
            .FirstOrDefault(p => p.Name == PrintButtonName);
        if (newPrintButton != null)
            newPrintButton.Command = PrintCommand;
        
        var newImportDropDownButtonAdv = VisualTreeHelperEx
            .FindVisualChildren<DropDownButtonAdv>(AssociatedObject)
            .FirstOrDefault(p => p.Name == ImportDropDownButtonAdvName);
        if (newImportDropDownButtonAdv != null)
            AttachImportDropDownButton(newImportDropDownButtonAdv);
        
        var newExportDropDownButtonAdv = VisualTreeHelperEx
            .FindVisualChildren<DropDownButtonAdv>(AssociatedObject)
            .FirstOrDefault(p => p.Name == ExportDropDownButtonAdvName);
        if (newExportDropDownButtonAdv != null)
            AttachExportDropDownButton(newExportDropDownButtonAdv);
        
        var newSearchPopup = VisualTreeHelperEx
            .FindVisualChildren<Popup>(AssociatedObject)
            .FirstOrDefault(tb => tb.Name == SearchPopupName);
        if (newSearchPopup != null)
            AttachToSearchPopup(newSearchPopup);
        
        var newColumnSettingsButton = VisualTreeHelperEx
            .FindVisualChildren<ButtonBase>(AssociatedObject)
            .FirstOrDefault(p => p.Name == ColumnSettingsButtonName);
        if (newColumnSettingsButton != null)
            AttachToColumnSettingsButton(newColumnSettingsButton);
    }
    
    private void ImportDropDownButton_DropDownOpened(object? sender, EventArgs e)
    {
        if (sender is not DropDownButtonAdv dropDownButtonAdv)
            return;

        if (dropDownButtonAdv.Template.FindName(DropDownButtonPopupName, dropDownButtonAdv) is not Popup popup)
            return;

        if (popup.Child is not FrameworkElement root)
            return;

        var importFromExcelMenuItem = VisualTreeHelperEx
            .FindVisualChildren<DropDownMenuItem>(root)
            .FirstOrDefault(mi => mi.Name == ImportFromExcelDropDownMenuItemName);

        if (importFromExcelMenuItem != null)
            importFromExcelMenuItem.Command = ImportFromExcelCommand;
    }

    private void ExportDropDownButton_DropDownOpened(object? sender, EventArgs e)
    {
        if(sender is not DropDownButtonAdv dropDownButtonAdv)
            return;
        
        if(dropDownButtonAdv.Template.FindName(DropDownButtonPopupName, dropDownButtonAdv) is not Popup popup)
            return;
        
        if(popup.Child is not FrameworkElement root)
            return;
        
        var exportToExcelMenuItem = VisualTreeHelperEx
            .FindVisualChildren<DropDownMenuItem>(root)
            .FirstOrDefault(mi => mi.Name == ExportToExcelDropDownMenuItemName);
        if (exportToExcelMenuItem != null)
            exportToExcelMenuItem.Command = ExportToExcelCommand;
        
        var exportToPdfMenuItem = VisualTreeHelperEx
            .FindVisualChildren<DropDownMenuItem>(root)
            .FirstOrDefault(mi => mi.Name == ExportToPdfDropDownMenuItemName);
        if(exportToPdfMenuItem != null)
            exportToPdfMenuItem.Command = ExportToPdfCommand;
    }
    
    private void SearchPopup_OnOpened(object? sender, EventArgs e)
    {
        if (sender is not Popup { Child: FrameworkElement root })
            return;
        
        var newSearchTypeDropDownButtonAdv = VisualTreeHelperEx
            .FindVisualChildren<DropDownButtonAdv>(root)
            .FirstOrDefault(tb => tb.Name == SearchTypeDropDownButtonAdvName);
        if (newSearchTypeDropDownButtonAdv != null)
            AttachToSearchTypeDropDownButtonAdv(newSearchTypeDropDownButtonAdv);

        var newTextBox = VisualTreeHelperEx
            .FindVisualChildren<TextBox>(root)
            .FirstOrDefault(tb => tb.Name == SearchTextBoxName);
        if (newTextBox != null)
            AttachToSearchTextBox(newTextBox);
        
        var newPreviousSearchResultButton = VisualTreeHelperEx
            .FindVisualChildren<ButtonBase>(root)
            .FirstOrDefault(b => b.Name == PreviousSearchResultButtonName);
        if (newPreviousSearchResultButton != null)
            AttachToPreviousSearchResultButton(newPreviousSearchResultButton);
        
        var newNextSearchResultButton = VisualTreeHelperEx
            .FindVisualChildren<ButtonBase>(root)
            .FirstOrDefault(b => b.Name == NextSearchResultButtonName);
        if(newNextSearchResultButton != null)
            AttachToNextSearchResultButton(newNextSearchResultButton);
        
        var newClearSearchButton = VisualTreeHelperEx
            .FindVisualChildren<ButtonBase>(root)
            .FirstOrDefault(b => b.Name == ClearSearchButtonName);
        if(newClearSearchButton != null)
            AttachToClearSearchButton(newClearSearchButton);
    }

    private void AttachToSearchTypeDropDownButtonAdv(DropDownButtonAdv dropDownButtonAdv)
    {
        if(_searchTypeDropDownButtonAdv != null)
            _searchTypeDropDownButtonAdv.DropDownOpened -= SearchTypeDropDownButtonAdv_DropDownOpened;
        
        _searchTypeDropDownButtonAdv = dropDownButtonAdv;
        _searchTypeDropDownButtonAdv.DropDownOpened += SearchTypeDropDownButtonAdv_DropDownOpened;
    }

    private void SearchTypeDropDownButtonAdv_DropDownOpened(object? sender, EventArgs e)
    {
        if(sender is not DropDownButtonAdv dropDownButtonAdv)
            return;
        
        if(dropDownButtonAdv.Template.FindName(DropDownButtonPopupName, dropDownButtonAdv) is not Popup popup)
            return;
        
        if(popup.Child is not FrameworkElement root)
            return;

        var startWidthDropDownItem = VisualTreeHelperEx
            .FindVisualChildren<DropDownMenuItem>(root)
            .FirstOrDefault(p => p.Name == StartWithDropDownItemName);
        if (startWidthDropDownItem != null)
        {
            startWidthDropDownItem.ApplyTemplate();
            
            if (startWidthDropDownItem.Template.FindName(StartWithRadioButtonName, startWidthDropDownItem) is RadioButton startWithRadioButton)
            {
                AttachToStartWithRadioButton(startWithRadioButton);
            }
        }

        var containsDropDownItem = VisualTreeHelperEx
            .FindVisualChildren<DropDownMenuItem>(root)
            .FirstOrDefault(p => p.Name == ContainsDropDownItemName);
        if (containsDropDownItem != null)
        {
            containsDropDownItem.ApplyTemplate();
            
            if (containsDropDownItem.Template.FindName(ContainsRadioButtonName, containsDropDownItem) is RadioButton containsWithRadioButton)
            {
                AttachToContainsRadioButton(containsWithRadioButton);
            }
        }
        
        var endWithDropDownItem = VisualTreeHelperEx
            .FindVisualChildren<DropDownMenuItem>(root)
            .FirstOrDefault(p => p.Name == EndWithDropDownItemName);
        if (endWithDropDownItem != null)
        {
            endWithDropDownItem.ApplyTemplate();
            
            if (endWithDropDownItem.Template.FindName(EndWithRadioButtonName, endWithDropDownItem) is RadioButton endWithRadioButton)
            {
                AttachToEndWithRadioButton(endWithRadioButton);
            }
        }
    }

    private void AttachToStartWithRadioButton(RadioButton startWithRadioButton)
    {
        if(_startWithRadioButton != null)
            _startWithRadioButton.Checked -= SearchTypeRadioButton_Checked;
        
        _startWithRadioButton = startWithRadioButton;
        _startWithRadioButton.Checked += SearchTypeRadioButton_Checked;
    }
    
    private void AttachToContainsRadioButton(RadioButton containsRadioButton)
    {
        if(_containsRadioButton != null)
            _containsRadioButton.Checked -= SearchTypeRadioButton_Checked;
        
        _containsRadioButton = containsRadioButton;
        _containsRadioButton.Checked += SearchTypeRadioButton_Checked;
    }
    
    private void AttachToEndWithRadioButton(RadioButton endWithRadioButton)
    {
        if(_endWithRadioButton != null)
            _endWithRadioButton.Checked -= SearchTypeRadioButton_Checked;
        
        _endWithRadioButton = endWithRadioButton;
        _endWithRadioButton.Checked += SearchTypeRadioButton_Checked;
    }


    private void SearchTypeRadioButton_Checked(object? sender, RoutedEventArgs e)
    {
        if(sender is not RadioButton radioButton)
            return;
        
        if(radioButton.Tag is not SearchType searchType)
            return;
        
        SearchTypeChanged(searchType);
    }
    
    private void AttachToSearchTextBox(TextBox textBox)
    {
        if (_searchTextBox != null)
            _searchTextBox.TextChanged -= SearchTextBox_OnTextChanged;

        _searchTextBox = textBox;
        _searchTextBox.TextChanged += SearchTextBox_OnTextChanged;
    }

    private void AttachToNextSearchResultButton(ButtonBase button)
    {
        if(_nextSearchResultButton != null)
            _nextSearchResultButton.Click -= NextSearchResultButton_OnClick;
        
        _nextSearchResultButton = button;
        _nextSearchResultButton.Click += NextSearchResultButton_OnClick;
    }

    private void AttachToPreviousSearchResultButton(ButtonBase button)
    {
        if(_previousSearchResultButton != null)
            _previousSearchResultButton.Click -= PreviousSearchResultButton_OnClick;
        
        _previousSearchResultButton = button;
        _previousSearchResultButton.Click += PreviousSearchResultButton_OnClick;
    }

    private void AttachToClearSearchButton(ButtonBase button)
    {
        if(_clearSearchButton != null)
            _clearSearchButton.Click -= ClearSearchButton_OnClick;
        
        _clearSearchButton = button;
        _clearSearchButton.Click += ClearSearchButton_OnClick;
    }
    
    private void SearchTextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if(sender is not TextBox searchTextBox)
            return;
        
        _searchText = searchTextBox.Text;

        if (_searchDebounceTimer == null)
        {
            _searchDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DebounceMsInterval)
            };
            _searchDebounceTimer.Tick += (_, _) =>
            {
                _searchDebounceTimer?.Stop();
                SearchHelper?.Search(_searchText);
            };
        }
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    private void PreviousSearchResultButton_OnClick(object sender, RoutedEventArgs e)
    {
        SearchHelper?.FindPrevious(_searchText);
    }
    
    private void NextSearchResultButton_OnClick(object sender, RoutedEventArgs e)
    {
        SearchHelper?.FindNext(_searchText);
    }

    private void ClearSearchButton_OnClick(object sender, RoutedEventArgs e)
    {
        ClearSearch();
    }
    
    #endregion
}