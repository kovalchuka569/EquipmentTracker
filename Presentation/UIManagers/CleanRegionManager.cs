using System.Linq;
using Presentation.Interfaces;
using Prism.Navigation.Regions;

namespace Presentation.UIManagers;

public class CleanRegionManager : ICleanRegionManager
{
    public void CleanUpRegions(IRegionManager regionManager)
    {
        var regionCopies = regionManager.Regions.ToList();
        
        foreach (var region in regionCopies)
        {
            region.RemoveAll();
        }
    }
}