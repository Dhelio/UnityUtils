using System.Collections.Generic;

namespace CastrimarisStudios.Extensions {

    public static class ListExtensions {
      
        /// <summary>
        /// Fills the target list (if there are enough elements) with the Source to Capacity.
        /// </summary>
        /// <param name="Source">The list to fill the target with</param>
        /// <returns>Count of inserted items</returns>
        public static int FillWith<T>(this List<T> target, List<T> Source) {
            int fillCount = 0;
            while (target.Count < target.Capacity) {
                target.Add(Source[fillCount]);
                fillCount++;
            }
            return fillCount;
        }

        /// <summary>
        /// Fills the Destination list to Capacity (if there are enough elements) with this list.
        /// </summary>
        /// <param name="Destination">The list to fill</param>
        /// <returns>Count of inserted items</returns>
        public static int Fill<T>(this List<T> target, List<T> Destination) {
            int fillCount = 0;
            while (Destination.Count < Destination.Capacity) {
                Destination.Add(target[fillCount]);
                fillCount++;
            }
            return fillCount;
        }

        /// /// <summary>
        /// Fills the Destination list to Capacity with this list.
        /// </summary>
        /// <param name="Destination">The list to fill</param>
        /// <param name="CanHaveRepeatedElements"></param>
        /// <returns>Count of inserted items</returns>
        public static int FillRandomly<T>(this List<T> target, List<T> Destination, bool CanHaveRepeatedElements = false) {
            int fillCount = 0;
            var rng = new System.Random();
            while (Destination.Count < Destination.Capacity) {
                var index = rng.Next(0, target.Count);
                if (CanHaveRepeatedElements || !Destination.Contains(target[index])) {
                    Destination.Add(target[index]);
                    fillCount++;
                }
            }
            return fillCount;
        }

    }

}
