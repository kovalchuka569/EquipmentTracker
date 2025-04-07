using System.Windows;

namespace Core.Services.TabControlExt;

public interface IRegionManagerService
    {
        IRegionManager CreateRegionManagerScope(FrameworkElement scopingElement, string scopeName);
        string GetRegionName(string baseRegionName);
        void CleanupRegionManager(FrameworkElement scopingElement);
    }
    