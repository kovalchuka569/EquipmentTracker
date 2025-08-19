using Prism.Navigation.Regions;

namespace Presentation.Interfaces;

public interface ICleanRegionManager
{
    void CleanUpRegions(IRegionManager regionManager);
}