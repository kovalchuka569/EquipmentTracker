using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

using Microsoft.Xaml.Behaviors;

using Syncfusion.PivotAnalysis.Base;
using Syncfusion.Windows.Controls.PivotGrid;

namespace Presentation.Behaviors;

public class PivotGridControlBehavior : Behavior<PivotGridControl>
{
    public static readonly DependencyProperty PivotColumnsProperty =
        DependencyProperty.Register(
            nameof(PivotColumns), 
            typeof(ObservableCollection<PivotItem>), 
            typeof(PivotGridControlBehavior), 
            new PropertyMetadata(null, OnPivotColumnsChanged));
    
    public static readonly DependencyProperty PivotRowsProperty =
        DependencyProperty.Register(
            nameof(PivotRows), 
            typeof(ObservableCollection<PivotItem>), 
            typeof(PivotGridControlBehavior), 
            new PropertyMetadata(null, OnPivotRowsChanged));
    
    public static readonly DependencyProperty PivotCalculationsProperty =
        DependencyProperty.Register(
            nameof(PivotCalculations), 
            typeof(ObservableCollection<PivotComputationInfo>), 
            typeof(PivotGridControlBehavior), 
            new PropertyMetadata(null, OnPivotCalculationsChanged));

    public ObservableCollection<PivotItem>? PivotColumns
    {
        get => (ObservableCollection<PivotItem>)GetValue(PivotColumnsProperty);
        set => SetValue(PivotColumnsProperty, value);
    }
    
    public ObservableCollection<PivotItem>? PivotRows
    {
        get => (ObservableCollection<PivotItem>)GetValue(PivotRowsProperty);
        set => SetValue(PivotRowsProperty, value);
    }

    public ObservableCollection<PivotComputationInfo>? PivotCalculations
    {
        get => (ObservableCollection<PivotComputationInfo>)GetValue(PivotCalculationsProperty);
        set => SetValue(PivotCalculationsProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        
        // Подписываемся на события для отслеживания изменений
        if (AssociatedObject != null)
        {
            AssociatedObject.Loaded += OnPivotGridLoaded;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.Loaded -= OnPivotGridLoaded;
        }
        
        // Отписываемся от событий коллекций
        UnsubscribeFromCollectionEvents();
        
        base.OnDetaching();
    }

    private void OnPivotGridLoaded(object sender, RoutedEventArgs e)
    {
        PopulateAllCollections();
        SubscribeToCollectionEvents();
    }

    private void PopulateAllCollections()
    {
        if (AssociatedObject == null) return;

        PopulatePivotRows();
        PopulatePivotColumns();
        PopulatePivotCalculations();
    }

    private void SubscribeToCollectionEvents()
    {
        if (PivotRows != null)
            PivotRows.CollectionChanged += OnPivotRowsCollectionChanged;
            
        if (PivotColumns != null)
            PivotColumns.CollectionChanged += OnPivotColumnsCollectionChanged;
            
        if (PivotCalculations != null)
            PivotCalculations.CollectionChanged += OnPivotCalculationsCollectionChanged;
    }

    private void UnsubscribeFromCollectionEvents()
    {
        if (PivotRows != null)
            PivotRows.CollectionChanged -= OnPivotRowsCollectionChanged;
            
        if (PivotColumns != null)
            PivotColumns.CollectionChanged -= OnPivotColumnsCollectionChanged;
            
        if (PivotCalculations != null)
            PivotCalculations.CollectionChanged -= OnPivotCalculationsCollectionChanged;
    }

    private static void OnPivotColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PivotGridControlBehavior behavior)
        {
            if (e.OldValue is ObservableCollection<PivotItem> oldColumns)
                oldColumns.CollectionChanged -= behavior.OnPivotColumnsCollectionChanged;
            
            if (e.NewValue is ObservableCollection<PivotItem> newColumns)
                newColumns.CollectionChanged += behavior.OnPivotColumnsCollectionChanged;
                
            behavior.PopulatePivotColumns();
        }
    }
    
    private static void OnPivotRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PivotGridControlBehavior behavior)
        {
            // Отписываемся от старой коллекции
            if (e.OldValue is ObservableCollection<PivotItem> oldRows)
                oldRows.CollectionChanged -= behavior.OnPivotRowsCollectionChanged;
                
            // Подписываемся на новую коллекцию
            if (e.NewValue is ObservableCollection<PivotItem> newRows)
                newRows.CollectionChanged += behavior.OnPivotRowsCollectionChanged;
                
            behavior.PopulatePivotRows();
        }
    }
    
    private static void OnPivotCalculationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PivotGridControlBehavior behavior)
        {
            // Отписываемся от старой коллекции
            if (e.OldValue is ObservableCollection<PivotComputationInfo> oldCalculations)
                oldCalculations.CollectionChanged -= behavior.OnPivotCalculationsCollectionChanged;
                
            // Подписываемся на новую коллекцию
            if (e.NewValue is ObservableCollection<PivotComputationInfo> newCalculations)
                newCalculations.CollectionChanged += behavior.OnPivotCalculationsCollectionChanged;
                
            behavior.PopulatePivotCalculations();
        }
    }

    private void PopulatePivotRows()
    {
        if (AssociatedObject?.PivotRows == null || PivotRows == null) return;
        
        AssociatedObject.PivotRows.Clear();
        foreach (var row in PivotRows)
        {
            AssociatedObject.PivotRows.Add(row);
        }
    }

    private void PopulatePivotColumns()
    {
        if (AssociatedObject?.PivotColumns == null || PivotColumns == null) return;
        
        AssociatedObject.PivotColumns.Clear();
        foreach (var column in PivotColumns)
        {
            AssociatedObject.PivotColumns.Add(column);
        }
    }

    private void PopulatePivotCalculations()
    {
        if (AssociatedObject?.PivotCalculations == null || PivotCalculations == null) return;
        
        AssociatedObject.PivotCalculations.Clear();
        foreach (var calculation in PivotCalculations)
        {
            AssociatedObject.PivotCalculations.Add(calculation);
        }
    }

    private void OnPivotRowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PopulatePivotRows();
    }

    private void OnPivotColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PopulatePivotColumns();
    }

    private void OnPivotCalculationsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PopulatePivotCalculations();
    }
}