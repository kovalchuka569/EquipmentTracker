using System.Windows;

namespace Core.Services.TabControlExt;

 public class RegionManagerService : IRegionManagerService
    {
        private readonly IRegionManager _globalRegionManager;
        private readonly Dictionary<string, IRegionManager> _scopedManagers = new Dictionary<string, IRegionManager>();
        
        public RegionManagerService(IRegionManager regionManager)
        {
            _globalRegionManager = regionManager;
        }

        /// <summary>
        /// Creates a new RegionManager scope and binds it to the element
        /// </summary>
        /// <param name="scopingElement">The element to which the scope is attached</param>
        /// <param name="scopeName">Unique scope name</param>
        /// <returns>Scoped RegionManager</returns>
        public IRegionManager CreateRegionManagerScope(FrameworkElement scopingElement, string scopeName)
        {
            if (scopingElement == null)
                throw new ArgumentNullException(nameof(scopingElement));
            
            if (string.IsNullOrEmpty(scopeName))
                scopeName = Guid.NewGuid().ToString();
            
            Console.WriteLine($"Creating scoped RegionManager: {scopeName}");
            
            // Create a new RegionManager scoop
            var scopedRegionManager = _globalRegionManager.CreateRegionManager();
            
            // Save in dictionary
            _scopedManagers[scopeName] = scopedRegionManager;
            
            // Bind to the element
            RegionManager.SetRegionManager(scopingElement, scopedRegionManager);
            
            // Store the scope name in the element property for later clearing
            scopingElement.SetValue(RegionManagerProperty, scopeName);
            
            return scopedRegionManager;
        }

        /// <summary>
        /// Returns the default region name (without having to generate a GUID)
        /// </summary>
        /// <param name="baseRegionName">Default region name</param>
        /// <returns>Region name</returns>
        public string GetRegionName(string baseRegionName)
        {
            return baseRegionName;
        }

        /// <summary>
        /// Clears the scoped RegionManager attached to an element.
        /// </summary>
        /// <param name="scopingElement">The element from which the RegionManager needs to be unbound</param>
        public void CleanupRegionManager(FrameworkElement scopingElement)
        {
            if (scopingElement == null)
                return;
            
            // Get the scope name from the element property
            var scopeName = scopingElement.GetValue(RegionManagerProperty) as string;
            if (string.IsNullOrEmpty(scopeName))
                return;
            
            Console.WriteLine($"Cleaning up scoped RegionManager: {scopeName}");
            
            // Get the scoped RegionManager
            if (_scopedManagers.TryGetValue(scopeName, out var scopedManager))
            {
                // Clear all regions in this scope
                foreach (var region in scopedManager.Regions)
                {
                    region.RemoveAll();
                }
                
                // Remove from dictionary
                _scopedManagers.Remove(scopeName);
                
                // Unbind from the element
                RegionManager.SetRegionManager(scopingElement, null);
                scopingElement.ClearValue(RegionManagerProperty);
                
                Console.WriteLine($"RegionManager {scopeName} cleaned up successfully");
            }
        }
        
        // Dependency property for storing scope name
        public static readonly DependencyProperty RegionManagerProperty =
            DependencyProperty.RegisterAttached("RegionManagerScope", typeof(string), typeof(RegionManagerService), 
                new PropertyMetadata(null));
    }