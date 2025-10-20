using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Presentation.Services;
using Presentation.Models;
using Syncfusion.UI.Xaml.Grid;

namespace Presentation.Behaviors;

public class SfDataGridPrintBehavior : Behavior<SfDataGrid>
{
    public static readonly DependencyProperty GridPrintCommandProperty = DependencyProperty.Register(
        nameof(GridPrintCommand),
        typeof(ICommand), 
        typeof(SfDataGridPrintBehavior), 
        new PropertyMetadata(null, OnGridPrintCommandChanged));

    public static readonly DependencyProperty GridPrintParametersProperty = DependencyProperty.Register(
        nameof(GridPrintParameters),
        typeof(GridPrintParameters), typeof(SfDataGridPrintBehavior),
        new PropertyMetadata(null, OnPrintParametersChanged));
    
    private GridPrintParameters _pendingGridPrintParameters = new();
    
    public ICommand? GridPrintCommand
    {
        get => (ICommand)GetValue(GridPrintCommandProperty);
        set => SetValue(GridPrintCommandProperty, value);
    }

    public GridPrintParameters? GridPrintParameters
    {
        get => (GridPrintParameters)GetValue(GridPrintParametersProperty);
        set => SetValue(GridPrintParametersProperty, value);
    }

    private static void OnGridPrintCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (SfDataGridPrintBehavior)d;

        if (e.OldValue is ICommand oldCommand) oldCommand.CanExecuteChanged -= behavior.OnCanExecuteChanged;

        if (e.NewValue is ICommand newCommand) newCommand.CanExecuteChanged += behavior.OnCanExecuteChanged;
    }

    private void OnCanExecuteChanged(object? sender, System.EventArgs e)
    {
        if (GridPrintCommand?.CanExecute(null) == true) AssociatedObject?.ShowPrintPreview();
    }

    private static void OnPrintParametersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (SfDataGridPrintBehavior)d;
        if (e.NewValue is not GridPrintParameters newParameters)
            return;

        behavior.UpdatePrintManager(newParameters);
    }

    private void UpdatePrintManager(GridPrintParameters newParameters)
    {
        if (AssociatedObject.View is null)
        {
            _pendingGridPrintParameters = newParameters;
            AssociatedObject.ItemsSourceChanged += OnDataGridLoadedForPrint;
            return;
        }
        
        CreatePrintManager(_pendingGridPrintParameters);
    }

    private void OnDataGridLoadedForPrint(object? sender, GridItemsSourceChangedEventArgs e)
    {
        AssociatedObject.ItemsSourceChanged -= OnDataGridLoadedForPrint;
        CreatePrintManager(_pendingGridPrintParameters);
    }
    
    private void CreatePrintManager(GridPrintParameters newParameters)
    {
        // TODO: Make normal printing work (templates, font settings, etc.) as requested here.
        // No parameters are currently applied (the task is not precise). 
        // SyncfusionGridPrintManager has default settings (plug)
        
        
        AssociatedObject.PrintSettings.PrintManagerBase = new SyncfusionGridPrintManager(AssociatedObject);
        
    }

    protected override void OnDetaching()
    {
        if (GridPrintCommand is not null) GridPrintCommand.CanExecuteChanged -= OnCanExecuteChanged;

        base.OnDetaching();
    }
}