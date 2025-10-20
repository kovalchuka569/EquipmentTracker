using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.TextInputLayout;

namespace Common.Behaviors;

public class SfTextInputLayoutValidationBehavior : Behavior<SfTextInputLayout>
{
    private TextBox? _textBox;

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.Loaded += OnLoaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.Loaded -= OnLoaded;

        if (_textBox != null)
            Validation.RemoveErrorHandler(_textBox, OnValidationError);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject.InputView is not TextBox textBox) return;
        _textBox = textBox;
        Validation.AddErrorHandler(_textBox, OnValidationError);
    }

    private void OnValidationError(object? sender, ValidationErrorEventArgs e)
    {
        var hasError = false;
        var errors = new ReadOnlyObservableCollection<ValidationError>([]);

        if (_textBox is not null)
        {
            hasError = Validation.GetHasError(_textBox);
            errors = Validation.GetErrors(_textBox);
        }

        AssociatedObject.HasError = hasError;
        AssociatedObject.ErrorText =
            hasError ? errors.FirstOrDefault()?.ErrorContent?.ToString() ?? string.Empty : string.Empty;
    }
}