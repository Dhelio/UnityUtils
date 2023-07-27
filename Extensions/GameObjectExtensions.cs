using UnityEngine;

namespace Castrimaris.Core {
	public static class GameObjectExtensions {

		/// <summary>
		/// Searches for a component in this GameObject or its children
		/// </summary>
		/// <typeparam name="T">Type of the component to search for.</typeparam>
		/// <param name="target">The object on which to search</param>
		/// <returns>The component if found, null otherwise.</returns>
		public static T FindComponent<T>(this GameObject target) {
			T result = target.GetComponent<T>();
			if (result == null) {
				result = target.GetComponentInChildren<T>();
			}
			return result;
		}

	}
}
