using System.Collections.Generic;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extension methods for Generic collections
    /// </summary>
    public static class CollectionsExtensions {

        /// <summary>
        /// Wheter or not this array is null or empty.
        /// </summary>
        /// <typeparam name="T">Type of the array</typeparam>
        /// <param name="target">The target of the check</param>
        /// <returns>True if it's null or empty, False otherwise</returns>
        public static bool IsNullOrEmpty<T>(this T[] target) => target == null || target.Length == 0;

        /// <summary>
        /// Wheter or not this list is null or empty.
        /// </summary>
        /// <typeparam name="T">Type of the list</typeparam>
        /// <param name="target">The target of the check</param>
        /// <returns>True if it's null or empty, False otherwise</returns>
        public static bool IsNullOrEmpty<T>(this IList<T> target) => target == null || target.Count == 0;

        /// <summary>
        /// Wheter or not this array is null or empty.
        /// </summary>
        /// <typeparam name="T">Type of the array</typeparam>
        /// <param name="target">The target of the check</param>
        /// <returns>True if it's NOT null or empty, False otherwise</returns>
        public static bool NotNullOrEmpty<T>(this T[] target) => !target.IsNullOrEmpty();

        /// <summary>
        /// Wheter or not this list is null or empty.
        /// </summary>
        /// <typeparam name="T">Type of the list</typeparam>
        /// <param name="target">The target of the check</param>
        /// <returns>True if it's NOT null or empty, False otherwise</returns>
        public static bool NotNullOrEmpty<T>(this IList<T> target) => !target.IsNullOrEmpty();

    }

}