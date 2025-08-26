using System;
using System.Threading.Tasks;

using Prism.Commands;

namespace Presentation.Extensions;

public static class DelegateCommandExtensions
{
    public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool>? canExecuteMethod = null)
    {
        return canExecuteMethod == null
            ? new DelegateCommand(() => executeMethod().FireAndForgetSafeAsync())
            : new DelegateCommand(() => executeMethod().FireAndForgetSafeAsync(), canExecuteMethod);
    }
    
    public static DelegateCommand<T> FromAsyncHandler<T>(Func<T, Task> executeMethod, Func<T, bool>? canExecuteMethod = null)
    {
        return canExecuteMethod == null
            ? new DelegateCommand<T>(param => executeMethod(param).FireAndForgetSafeAsync())
            : new DelegateCommand<T>(param => executeMethod(param).FireAndForgetSafeAsync(), canExecuteMethod);
    }
    
    private static async void FireAndForgetSafeAsync(this Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}