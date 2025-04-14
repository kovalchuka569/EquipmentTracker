﻿namespace UI.Services.Logging
{
    public interface IAppLogger<T>
    {
        void LogDebug(string message, params object[] args);
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogError(string message, params object[] args);
        void LogCritical(Exception exception, string message, params object[] args);
    }
}
