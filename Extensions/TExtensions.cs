using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CastrimarisStudios.Extensions {
  
    public static class TExtensions {
      
        /// <summary>
        /// Performs a Sanity Check on this component. If it's null, then it tries to find it with a GameObject.Find(); if that is null too, throws NullReference exception. Best used in Awake().
        /// </summary>
        /// <param name="GameObjectName">The name of the object to look for in case it is null.</param>
        /// <exception cref="System.NullReferenceException">The searched reference doesn't exist.</exception>
		public static void SanityCheck<T>(this T target, string GameObjectName) {
            if (target == null) {
                target = GameObject.Find(GameObjectName).GetComponent<T>();
                if (target == null) {
                    throw new System.NullReferenceException($"Could not find a reference for {GameObjectName}. Did you forget to assign it in the Editor?");
                }
            }
        }

    }
}
