using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Core {
    public static class TransformExtensions {

        /// <summary>
        /// Looks for a child <see cref="Transform"/> recursively.
        /// </summary>
        /// <param name="Name">The name of the child <see cref="Transform"/>. Can be any nested child of this <see cref="Transform"/></param>
        /// <returns>The child <see cref="Transform"/> if found, null otherwise.</returns>
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
        /// Looks for a parent <see cref="Transform"/> recursively.
        /// </summary>
        /// <param name="Name">The name of the parent transform to look for. Can be any nested parent of this <see cref="Transform"/>.</param>
        /// <returns>The parent <see cref="Transform"/> if found, null otherwise</returns>
        public static Transform RecursiveFindParent(this Transform target, string Name) {
            var name = target.parent.name;
            if (target.parent.name == Name) {
                return target.parent;
            } else if (target.parent != null) {
                return target.parent.RecursiveFindParent(Name);
            }
            return null;
        }


        /// <summary>
        /// Looks for multiple childs under this <see cref="Transform"/>.
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

        /// <summary>
        /// Destroys all childs of this <see cref="Transform"/> immediately. You are strogly recommended to use DestroyChilds instead.
        /// </summary>
        /// <returns>The number of destroyed childs.</returns>
        public static int DestroyChildsImmediate(this Transform target) {
            int i;
            for (i = 0; target.childCount > 0; i++) {
                GameObject.DestroyImmediate(target.GetChild(0).gameObject);
            }
            return i + 1;
        }

        /// <summary>
        /// Destroys all childs of this <see cref="Transform"/>.
        /// </summary>
        /// <returns>The number of destroyed childs.</returns>
        public static int DestroyChilds(this Transform target) {
            int i;
            for (i = 0; i < target.childCount; i++) {
                GameObject.Destroy(target.GetChild(i).gameObject);
            }
            return i + 1;
        }

        /// <summary>
        /// Sets all children of this <see cref="Transform"/> to the indicated active state.
        /// </summary>
        /// <param name="ChildrenActiveState">True if all childs should be active, false otherwise</param>
        public static void SetChildrenActiveState(this Transform target, bool ChildrenActiveState) {
            for (int i = 0; i < target.childCount; i++) {
                target.GetChild(i).gameObject.SetActive(ChildrenActiveState);
            }
        }

        /// <summary>
        /// Shuffles children order randomly.
        /// </summary>
        public static void ShuffleChildren(this Transform target) {
            System.Random rng = new System.Random();
            for (int i = 0; i < target.childCount; i++) {
                target.GetChild(i).SetSiblingIndex(rng.Next(0, target.childCount - 1));
            }
        }

        /// <summary>
        /// Checks if this <see cref="Transform"/> has a <see cref="Tags"/> component and compares tags.
        /// </summary>
        /// <param name="target">The target <see cref="Transform"/></param>
        /// <param name="Tags">Tags to check against.</param>
        /// <returns>True if the object has all tags.</returns>
        public static bool HasTags(this Transform target, params string[] Tags) {
            if (!target.gameObject.HasComponent<Tags>())
                return false;

            var tags = target.GetComponent<Tags>();
            return tags.Has(Tags);
        }

    }
}
