namespace Castrimaris.Addressables {

    public enum MergeModes {
        /// <summary>
        /// Use to indicate that no merge should occur. The first set of results will be used.
        /// </summary>
        None = 0,
        /// <summary>
        /// Use to indicate that the merge should take the first set of results.
        /// </summary>
        UseFirst = 0,
        /// <summary>
        /// Use to indicate that the merge should take the union of the results.
        /// </summary>
        Union,
        /// <summary>
        /// Use to indicate that the merge should take the intersection of the results.
        /// </summary>
        Intersection
    }

}