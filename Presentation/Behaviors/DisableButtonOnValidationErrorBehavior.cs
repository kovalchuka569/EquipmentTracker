using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace Presentation.Behaviors;

public class DisableButtonOnValidationErrorBehavior : Behavior<ButtonBase>
{
    public static readonly DependencyProperty ElementNamesProperty =
        DependencyProperty.Register(nameof(ElementNames), typeof(IEnumerable<string>),
            typeof(DisableButtonOnValidationErrorBehavior),
            new PropertyMetadata(null, OnElementNamesChanged));

    private readonly List<UIElement> _trackedElements = [];

    public IEnumerable<string> ElementNames
    {
        get => (IEnumerable<string>)GetValue(ElementNamesProperty);
        set => SetValue(ElementNamesProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
    }

    protected override void OnDetaching()
    {
        ClearTrackedElements();

        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;

        base.OnDetaching();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ResolveElementNames();
        UpdateTrackedElements();
        UpdateButtonState();
    }

    // Forced validation
    private void OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        if (_trackedElements.Count == 0)
            return;

        var hasError = false;

        foreach (var element in _trackedElements.OfType<TextBox>())
        {
            var binding = element.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            hasError = Validation.GetHasError(element);
        }

        if (hasError)
            e.Handled = true;
    }

    private static void OnElementNamesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (DisableButtonOnValidationErrorBehavior)d;

        if (behavior.AssociatedObject == null)
            return;

        behavior.ResolveElementNames();
        behavior.UpdateTrackedElements();
        behavior.UpdateButtonState();
    }

    private void ResolveElementNames()
    {
        _trackedElements.Clear();

        if (AssociatedObject is not FrameworkElement parent)
            return;

        foreach (var name in ElementNames)
        {
            var found = parent.FindName(name) as UIElement
                        ?? Window.GetWindow(parent)?.FindName(name) as UIElement;

            if (found != null)
                _trackedElements.Add(found);
        }
    }

    private void UpdateTrackedElements()
    {
        ClearTrackedElements();

        foreach (var element in _trackedElements)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(Validation.HasErrorProperty, typeof(UIElement));
            descriptor.AddValueChanged(element, OnValidationErrorChanged);
        }
    }

    private void ClearTrackedElements()
    {
        foreach (var element in _trackedElements)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(Validation.HasErrorProperty, typeof(UIElement));
            descriptor.RemoveValueChanged(element, OnValidationErrorChanged);
        }
    }

    private void OnValidationErrorChanged(object? sender, System.EventArgs e)
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (AssociatedObject == null)
            return;

        var hasAnyError = _trackedElements.Any(Validation.GetHasError);
        AssociatedObject.IsEnabled = !hasAnyError;
    }
}