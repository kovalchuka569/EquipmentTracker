using System.Windows;
using System.Windows.Media;

namespace Common.Extensions;

public static class VisualTreeHelperEx
{
    /// <summary>
    /// Recursively enumerates all descendant visual elements of the specified type
    /// starting from the given <see cref="DependencyObject"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of visual elements to search for. Must derive from <see cref="DependencyObject"/>.
    /// </typeparam>
    /// <param name="depObj">
    /// The root visual object from which the search begins. If <c>null</c>, the method returns an empty sequence.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> containing all descendant elements of type <typeparamref name="T"/>
    /// found in the visual tree, including those nested at any depth.
    /// </returns>
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
    {
        if (depObj == null) 
            yield break;
        
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            if (child is T dependencyObject)
                yield return dependencyObject;

            foreach (var childOfChild in FindVisualChildren<T>(child))
                yield return childOfChild;
        }
    }
}