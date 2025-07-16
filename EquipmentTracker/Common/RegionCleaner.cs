namespace EquipmentTracker.Common;

public class RegionCleaner
{
    public static void CleanUpRegions(IRegionManager? regionManager)
    {
        if (regionManager is null) return;
        
        var regionCopies = regionManager.Regions.ToList();
        
        foreach (var region in regionCopies)
        {
            region.RemoveAll();
        }
    }
}