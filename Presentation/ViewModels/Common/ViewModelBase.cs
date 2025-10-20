using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Presentation.ViewModels.Common;

public abstract class ViewModelBase
    : BindableBase, IDisposable, INotifyDataErrorInfo
{
    private bool _isInitialized;
    
    protected async Task ExecuteWithErrorHandlingAsync(
        Func<Task> action, 
        Action<Exception>? onError = null,
        Action? onFinally = null)
    {
        try
        {
            await action();
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
        }
        finally
        {
            onFinally?.Invoke();
        }
    }

    /// <summary>
    /// Executes an initialization action only once during the ViewModel's lifecycle.
    /// This method ensures that the provided async action is performed only on the first call,
    /// preventing redundant initialization operations when the ViewModel is reloaded or revisited.
    /// 
    /// Typical usage includes:
    /// - Loading initial data from databases or APIs
    /// - Setting up event handlers or subscriptions
    /// - Initializing collections or default values
    /// - Performing one-time configuration setup
    /// 
    /// Example:
    /// <code>
    /// protected override async void OnNavigatedTo(NavigationContext context)
    /// {
    ///     await InitializeAction(async () => 
    ///     {
    ///         await LoadUserDataAsync();
    ///         await InitializeSettingsAsync();
    ///         SetupEventHandlers();
    ///     });
    /// }
    /// </code>
    /// </summary>
    /// <param name="action">The asynchronous initialization action to execute. 
    /// This may include data loading, setup operations, or other one-time tasks.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    /// <remarks>
    /// Subsequent calls to this method after successful initialization will be ignored.
    /// If the action throws an exception, the initialization is considered incomplete
    /// and may be retried on subsequent calls (depending on error handling implementation).
    /// </remarks>
    protected async Task InitializeAction(Func<Task> action)
    {
        if(_isInitialized)
            return;

        await action();
        
        _isInitialized = true;
    }
    
    #region INotifyDataErrorInfo 

    private readonly Dictionary<string, List<string>> _errors = new();
    public bool HasErrors => _errors.Count != 0;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    
    public IEnumerable GetErrors(string? propertyName)
    {
        if(string.IsNullOrEmpty(propertyName))
            return _errors.SelectMany(kv => kv.Value);
        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }
    
    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = [];

        if (_errors[propertyName].Contains(error)) 
            return;
        
        _errors[propertyName].Add(error);
        RaiseErrorsChanged(propertyName);
    }
    
    protected void RemoveError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out var list) || !list.Remove(error)) 
            return;
        
        if (list.Count == 0)
            _errors.Remove(propertyName);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void RaiseErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
    
    #endregion

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}