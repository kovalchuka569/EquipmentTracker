using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Syncfusion.UI.Xaml.Grid;

namespace Presentation.Controls;

public partial class SyncfusionDataGridToolBar
{
    #region Dependency Properties
    public static readonly DependencyProperty GridSearchHelperProperty = DependencyProperty.Register(nameof(GridSearchHelper), typeof(SearchHelper), typeof(SyncfusionDataGridToolBar), new PropertyMetadata(null, OnSearchHelperChanged));

    public static readonly DependencyProperty ImportButtonVisibilityProperty = DependencyProperty.Register(nameof(ImportButtonVisibility), typeof(Visibility), typeof(SyncfusionDataGridToolBar), new PropertyMetadata(Visibility.Visible, OnImportButtonVisibilityChanged));
   
    public static readonly DependencyProperty ExcelImportCommandProperty = DependencyProperty.Register(nameof(ExcelImportCommand), typeof(ICommand), typeof(SyncfusionDataGridToolBar)); 
    
    public static readonly DependencyProperty PrintCommandProperty = DependencyProperty.Register(nameof(PrintCommand), typeof(ICommand), typeof(SyncfusionDataGridToolBar));

    public static readonly DependencyProperty ExcelExportCommandProperty = DependencyProperty.Register(nameof(ExcelExportCommand), typeof(ICommand), typeof(SyncfusionDataGridToolBar));
    
    public static readonly DependencyProperty PdfExportCommandProperty = DependencyProperty.Register(nameof(PdfExportCommand), typeof(ICommand), typeof(SyncfusionDataGridToolBar));
    #endregion
    
    #region Fields
    private CancellationTokenSource _searchCancellationTokenSource;
    
    private bool _isSearchTypeChanging;

    #endregion
    
    #region Properties
    public SearchHelper GridSearchHelper
    {
        get => (SearchHelper)GetValue(GridSearchHelperProperty);
        set => SetValue(GridSearchHelperProperty, value);
    }

    public Visibility ImportButtonVisibility
    {
        get => (Visibility)GetValue(ImportButtonVisibilityProperty);
        set => SetValue(ImportButtonVisibilityProperty, value);
    }

    public ICommand ExcelImportCommand
    {
        get => (ICommand)GetValue(ExcelImportCommandProperty);
        set => SetValue(ExcelImportCommandProperty, value);
    }

    public ICommand PrintCommand
    {
        get => (ICommand)GetValue(PrintCommandProperty);
        set => SetValue(PrintCommandProperty, value);
    }

    public ICommand ExcelExportCommand
    {
        get => (ICommand)GetValue(ExcelExportCommandProperty);
        set => SetValue(ExcelExportCommandProperty, value);
    }
    
    public ICommand PdfExportCommand
    {
        get => (ICommand)GetValue(PdfExportCommandProperty);
        set => SetValue(PdfExportCommandProperty, value);
    }
    #endregion
    
    #region Constructor
    public SyncfusionDataGridToolBar()
    {
        InitializeComponent();
        _searchCancellationTokenSource = new CancellationTokenSource();

        CheckDefaultSearchType();
    }
    #endregion
    
    #region Private methods
    
    private static void OnSearchHelperChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SyncfusionDataGridToolBar || e.NewValue is not SearchHelper searchHelper) return;
        
        var searchColor = (Color)ColorConverter.ConvertFromString("#09244B")!;
        searchHelper.SearchBrush = new SolidColorBrush(searchColor);
        searchHelper.SearchForegroundBrush = Brushes.White;
        searchHelper.AllowFiltering = true;
        searchHelper.SearchForegroundHighlightBrush = Brushes.White;
    }

    private static void OnImportButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyncfusionDataGridToolBar toolBar && e.NewValue is Visibility importButtonVisibility)
        {
            toolBar.ImportButton.Visibility = importButtonVisibility;
        }
    }
    
    private async void DebounceSearch(CancellationToken token)
    {
        try
        {
            await Task.Delay(500, token);
            if (!token.IsCancellationRequested)
            {
                GridSearchHelper.Search(SearchTextBox.Text);
            }
        }
        catch (TaskCanceledException) { }
    }
    
    private void UpdateWatermarkVisibility()
    {
        if (SearchTextBox.Template?.FindName("SearchWatermark", SearchTextBox) is TextBlock watermark)
        {
            watermark.Visibility = string.IsNullOrEmpty(SearchTextBox.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }
    }
    
    private void ClearSearch()
    {
        SearchTextBox.Text = string.Empty;
        GridSearchHelper.ClearSearch();
    }
    
    private void CheckDefaultSearchType()
    {
        _isSearchTypeChanging = true;
        ContainsSearchType.IsChecked = true;
        _isSearchTypeChanging = false;
    }
    
    #endregion
    
    #region Event handlers
    
    private void SearchSelector_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SearchTypeSelectorPopup.IsOpen = true;
    
    private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateWatermarkVisibility();
            
        _searchCancellationTokenSource.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();
        DebounceSearch(_searchCancellationTokenSource.Token);
    }
    
    private void SearchType_OnChecked(object sender, RoutedEventArgs e)
    {
        if (_isSearchTypeChanging || !(sender is RadioButton radioButton))
            return;
            
        try
        {
            ClearSearch();
            
            GridSearchHelper.SearchType = radioButton.Name switch
            {
                nameof(ContainsSearchType) => SearchType.Contains,
                nameof(StartWithSearchType) => SearchType.StartsWith,
                nameof(EndWithSearchType) => SearchType.EndsWith,
                _ => GridSearchHelper.SearchType
            };
        }
        finally
        {
            _isSearchTypeChanging = false;
        }
    }

    private void ClearSearch_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ClearSearch();
    }

    private void NavigationButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (string.IsNullOrEmpty(SearchTextBox.Text)) return;

        if (Equals(sender, PreviousButton))
            GridSearchHelper.FindPrevious(SearchTextBox.Text);
        else
            GridSearchHelper.FindNext(SearchTextBox.Text);
    }
    
    private void ExportButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ExportSelectorPopup.IsOpen = true;
    
    private void ImportButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ImportSelectorPopup.IsOpen = true;
    
    private void PrintButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        PrintCommand.Execute(null);
    }

    private void ExcelExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        ExportSelectorPopup.IsOpen = false;
        ExcelExportCommand.Execute(null);
    }

    private void PdfExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        ExportSelectorPopup.IsOpen = false;
        PdfExportCommand.Execute(null);
    }

    private void ExcelImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        ImportSelectorPopup.IsOpen = false;
        ExcelImportCommand.Execute(null);
    }
    
    #endregion
}