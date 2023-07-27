using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Core {
    public static class TransformExtensions {

        /// <summary>
        /// Looks for a child transform recursively.
        /// </summary>
        /// <param name="Name">The name of the child transform. Can be any nested child of this Transform</param>
        /// <returns>The child transform if found, null otherwise.</returns>
        public static Transform RecursiveFind(this Transform target, string Name) {
            foreach (Transform childTransform in target) {
                if (childTransform.name == Name) {
                    return childTransform;
                } else {
                    Transform found = RecursiveFind(childTransform, Name);
                    if (found != null) {
                        return found;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Looks for multiple childs under this transform.
        /// </summary>
        /// <param name="Exact">True if the search should stop when it has found exactly the number passed names, false if it should look for more same named children.</param>
        /// <param name="Names">Names of the childs to search</param>
        /// <returns></returns>
        public static Transform[] MultipleFind(this Transform target, bool Exact, params string[] Names) {
            List<Transform> result = new List<Transform>();
            List<string> names = Names.ToList();
            for (int i = 0; i < target.childCount; i++) {
                if (Names.Any(target.GetChild(i).name.Contains)) {
                    result.Add(target.GetChild(i));
                    if (Exact) {
                        if (names.Count == result.Count)
                            break;
                    }
                }
            }
            return result.ToArray();
        }

    }
}
