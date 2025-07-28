namespace Core.Common.RegionHelpers;

    /// <summary>
    /// Safely clears the contents of the specified region and removes the region itself from the RegionManager.
    /// </summary>
    /// <param name="regionManager">IRegionManager that contains the region being cleared</param>
    /// <param name="regionName">Region name to clear</param>
    public static class RegionCleanupHelper
    {
        public static void CleanRegion(IRegionManager regionManager, string regionName)
        {
            if (regionManager?.Regions == null) return;

            var region = regionManager.Regions.FirstOrDefault(r => r.Name == regionName);
            if (region == null) return;

            foreach (var view in region.Views.ToList())
            {
                region.Remove(view);
            }

            regionManager.Regions.Remove(regionName);
        }
    }