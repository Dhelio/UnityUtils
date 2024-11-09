namespace Castrimaris.Math {

    /// <summary>
    /// Math functions made in Castrimaris Group.
    /// </summary>
    public static class FMath {

        /// <summary>
        /// Wraps an integer between a minimum and a maximum
        /// </summary>
        /// <param name="Min">Minimum value that the int can assume</param>
        /// <param name="Max">Maximum value that the int can assume</param>
        /// <returns>Wrapped int</returns>
        public static int Clamp(int Target, int Min, int Max) {
            if (Target < Min) {
                Target = Min;
            } else if (Target > Max) {
                Target = Max;
            }
            return Target;
        }

        /// <summary>
        /// Wraps an integer between a minimum and a maximum
        /// </summary>
        /// <param name="Min">Minimum value that the int can assume</param>
        /// <param name="Max">Maximum value that the int can assume</param>
        public static void Clamp(ref int Target, int Min, int Max) {
            Target = Clamp(Target, Min, Max);
        }

        /// <summary>
        /// Wraps an integer between a minimum and a maximum; if the value is above maximum, then applies minimum; if it is under minimum, then applies maximum. Both min and max are inclusive.
        /// </summary>
        /// <param name="Min">Minimum value that the int can assume</param>
        /// <param name="Max">Maximum value that the int can assume</param>
        /// <returns>Wrapped int</returns>
        public static int EllipticClamp(int Target, int Min, int Max) {
            if (Target < Min) {
                Target = Max;
            } else if (Target > Max) {
                Target = Min;
            }
            return Target;
        }

        /// <summary>
        /// Wraps an integer between a minimum and a maximu; if the value is above maximum, then applies minimum; if it is under minimum, then applies maximum
        /// </summary>
        /// <param name="Min">Minimum value that the int can assume</param>
        /// <param name="Max">Maximum value that the int can assume</param>
        public static void EllipticClamp(ref int Target, int Min, int Max) {
            Target = EllipticClamp(Target, Min, Max);
        }

        /// <summary>
        /// Calculates how many characters have to change in a string in order to reach a destination.
        /// </summary>
        /// <param name="Source">The source string</param>
        /// <param name="Destination">The target string to reach</param>
        /// <returns>Count of how many chars have to change to turn source into target.</returns>
        public static int LevenshteinDistance(string Source, string Destination) {
            if (string.IsNullOrEmpty(Source)) {
                if (string.IsNullOrEmpty(Destination))
                    return 0;
                return Destination.Length;
            }

            if (string.IsNullOrEmpty(Destination)) {
                return Source.Length;
            }

            int n = Source.Length;
            int m = Destination.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++) {
                for (int j = 1; j <= m; j++) {
                    int cost = (Destination[j - 1] == Source[i - 1]) ? 0 : 1;
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