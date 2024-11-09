namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extension methods for <see cref="System.String"/>
    /// </summary>
    public static class StringExtensions {

        /// <summary>
        /// Shorthand for <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <returns>True if the parameter is null or an empty string (""); false otherwise.</returns>
        public static bool IsNullOrEmpty(this string target) => string.IsNullOrEmpty(target);

        /// <summary>
        /// Shorthand for inverse <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="target"></param>
        /// <returns>True if the parameter is NOT null or an empty string (""); false otherwise.</returns>
        public static bool NotNullOrEmpty(this string target) => !string.IsNullOrEmpty(target);

        /// <summary>
        /// Calculates how many characters have to change in a string in order to reach another destination string.
        /// </summary>
        /// <param name="source">The source string</param>
        /// <param name="target">The target string to reach</param>
        /// <returns>Count of how many chars have to change to turn source into target.</returns>
        public static int LevenshteinDistance(this string source, string target) {
            if (string.IsNullOrEmpty(source)) {
                if (string.IsNullOrEmpty(target))
                    return 0;
                return target.Length;
            }

            if (string.IsNullOrEmpty(target)) {
                return source.Length;
            }

            int n = source.Length;
            int m = target.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++) {
                for (int j = 1; j <= m; j++) {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = System.Math.Min(System.Math.Min(min1, min2), min3);
                }
            }

            return d[n, m];
        }

    }
}
