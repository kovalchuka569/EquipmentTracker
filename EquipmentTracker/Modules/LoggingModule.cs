using Microsoft.Extensions.Logging;
using Serilog;
using Common.Logging;
using UI.ViewModels.DataGrid;
using Prism.Modularity;
using Prism.Ioc;
using Prism.Unity;
using Unity;

namespace UI.Modules
{
    public class LoggingModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var container = containerProvider.GetContainer();
            
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
                builder.AddDebug();
                
                var serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File(
                        path: "logs/app-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 31,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
                    
                builder.AddSerilog(serilogLogger, dispose: true);
            });
            
            container.RegisterInstance(typeof(ILoggerFactory), loggerFactory);
            
            container.RegisterFactory(typeof(ILogger<>), (unityContainer, type, name) =>
            {
                var targetType = type.GetGenericArguments()[0];
                var lf = unityContainer.Resolve<ILoggerFactory>();
                var createLoggerMethod = typeof(LoggerFactoryExtensions)
                    .GetMethod("CreateLogger", new[] { typeof(ILoggerFactory) })
                    ?.MakeGenericMethod(targetType);
                
                return createLoggerMethod.Invoke(null, new object[] { lf });
            });
            
            container.RegisterType(typeof(IAppLogger<>), typeof(AppLogger<>));
        }
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //
        }
    }
}
