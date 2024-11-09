using UnityEngine;
using System.Collections.Generic;
using System;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extension methods for generics
    /// </summary>
    public static class TExtensions {

        /// <summary>
        /// Performs an action for each element of this array. Shorthand for <see cref="Array.ForEach{T}(T[], Action{T})"/>.
        /// </summary>
        /// <typeparam name="T">Type of the array to target</typeparam>
        /// <param name="action">The action to perform</param>
        public static void ForEach<T>(this T[] target, Action<T> action) => Array.ForEach(target, action);

        /// <summary>
        /// Performs an action for each element of the <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the IEnumerable</typeparam>
        /// <param name="target">The enumerable itself</param>
        /// <param name="action">The action to perform</param>
        public static void ForEach<T>(this IEnumerable<T> target, Action<T> action) {
            foreach (var item in target) {
                action(item);
            }
        }

        /// <summary>
        /// Looks for a specific component in this transform parents recursively.
        /// </summary>
        /// <typeparam name="T">Type of the Component to look for.</typeparam>
        /// <param name="target">The transform on which to perform the search</param>
        /// <returns>The component if found, null otherwise</returns>
        public static T GetComponentInParentRecursive<T>(this Component target) where T : Component {
            var result = target.GetComponentInParent<T>();
            if (result == null && target.transform.parent != null) {
                return target.transform.parent.GetComponentInParentRecursive<T>();
            }
            return result;
        }

        /// <summary>
        /// Looks for a specif interface in this component or its parents.
        /// </summary>
        /// <typeparam name="T">An interface</typeparam>
        /// <param name="target">The component where to perform the search.</param>
        /// <returns>The interface if found, null otherwise</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the passed type is not an interface.</exception>
        public static T GetInterfaceInParent<T>(this Component target) where T : class {
            //Type check
            if (!typeof(T).IsInterface) {
                throw new System.InvalidOperationException($"Could not retrieve interface for type {typeof(T)}, as it is not an interface");
            }

            return target.GetComponentInParent<T>();
        }

        /// <summary>
        /// Looks for a specif interface in this component or its parents.
        /// </summary>
        /// <typeparam name="T">An interface</typeparam>
        /// <param name="target">The component where to perform the search.</param>
        /// <returns>The interface if found, null otherwise</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the passed type is not an interface.</exception>
        public static T GetInterfaceInParent<T>(this GameObject target) where T : class {
            //Type check
            if (!typeof(T).IsInterface) {
                throw new System.InvalidOperationException($"Could not retrieve interface for type {typeof(T)}, as it is not an interface");
            }

            return target.GetComponentInParent<T>();
        }

        /// <summary>
        /// Looks for a specific inteface in this component parents recursively.
        /// </summary>
        /// <typeparam name="T">An interface</typeparam>
        /// <param name="target">The component where to perform the recursive search.</param>
        /// <returns>The interface if found, null otherwise</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the passed type is not an interface.</exception>
        public static T GetInterfaceInParentRecursive<T>(this Component target) where T : class {
            //Type check
            if (!typeof(T).IsInterface) {
                throw new System.InvalidOperationException($"Could not retrieve interface for type {typeof(T)}, as it is not an interface.");
            }

            //Local function that implements the recursion behaviour
            T GetInterfaceInParentRecursively(Component component) {
                var result = target.GetComponentInParent<T>();
                if (result == null && target.transform.parent != null) {
                    return GetInterfaceInParentRecursively(target.transform.parent);
                }
                return result;
            }

            return GetInterfaceInParentRecursively(target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="Result"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static bool TryGetInterface<T>(this Component target, out T Result) where T : class {
            //Type check
            if (!typeof(T).IsInterface) {
                throw new System.InvalidOperationException($"Could not retrieve interface for type {typeof(T)}, as it is not an interface.");
            }

            target.gameObject.TryGetComponent<T>(out Result);
            if (Result == null) {
                Result = target.gameObject.GetComponentInChildren<T>();
            }

            return Result != null;
        }

        /// <summary>
        /// Finds all child GameObjects that are on a specific Layer
        /// </summary>
        /// <typeparam name="T">A MonoBehaviour script</typeparam>
        /// <param name="LayerName">The name of the Layer to search the GameObjects in</param>
        /// <returns>A list of GameObjects. Possibly empty.</returns>
        public static List<GameObject> FindChildGameobjectsWithLayer<T>(this T target, string LayerName) where T : MonoBehaviour {
            List<GameObject> result = new List<GameObject>();

            void GetChildGameobjectsWithLayerRecursive(GameObject go, ref List<GameObject> list, string layerName) {
                if (go.layer == LayerMask.NameToLayer(layerName)) {
                    list.Add(go);
                }
                for (int i = 0; i < go.transform.childCount; i++) {
                    GetChildGameobjectsWithLayerRecursive(go.transform.GetChild(i).gameObject, ref list, layerName);
                }
            }

            GetChildGameobjectsWithLayerRecursive(target.gameObject, ref result, LayerName);

            return result;
        }
    }
}