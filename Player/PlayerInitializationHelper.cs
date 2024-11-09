using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Castrimaris.Player {

    /// <summary>
    /// Helper script to correctly initialize the <see cref="IPlayer"/> component.
    /// </summary>
    [RequireComponent(typeof(IPlayer))]
    [DisallowMultipleComponent]
    public class PlayerInitializationHelper : MonoBehaviour {

        #region Enums

        /// <summary>
        /// Types of systems
        /// </summary>
        public enum AdditionalSystemTypes { UNDEFINED = 0, UI, VR }

        #endregion

        #region Structs

        [System.Serializable]
        public struct AdditionalSystem {
            public AdditionalSystemTypes AdditionalSystemType;
            public GameObject Prefab;
        }

        #endregion

        #region Private Variables

        [Header("Parameters")]
        [Tooltip("[Experimental] Wheter in the " + nameof(gameObjectsToDestroy) + " field it should just destroy the components instead.")]
        [SerializeField] private bool destroyGameobjectsComponents = false;
        [Tooltip("How many times the script should attempt to remove Components and GameObjects when initializing (more than once because there might be RequireComponent or similar behaviour)")]
        [SerializeField, Range(1,1000)] private int maxRetries = 1000;

        [Header("References")]
        [Tooltip("Components that must be removed from the remote clients when the Player is networked.")]
        [SerializeField] private Component[] componentsToRemove;
        [Tooltip("GameObjects that must be destroyed from the remote clients")]
        [SerializeField] private GameObject[] gameObjectsToDestroy;
        [Tooltip("Additional components (with a root NetworkObject) that must be spawned after the Player and reparented.")]
        [SerializeField] private AdditionalSystem[] additionalSystems;

        [Header("Debug")]
        [SerializeField] private bool shouldSkip = false;

        #endregion

        #region PROPERTIES

        public GameObject[] AdditionalSystemPrefabs => (from system in additionalSystems select system.Prefab).ToArray() ?? null;
        public GameObject[] AdditionalNonUISystemPrefabs => (from system in additionalSystems where system.AdditionalSystemType != AdditionalSystemTypes.UI select system.Prefab).ToArray() ?? null;
        public GameObject MainUIAdditionalSystemPrefab => (from system in additionalSystems where system.AdditionalSystemType == AdditionalSystemTypes.UI select system.Prefab).FirstOrDefault() ?? null;
        public GameObject[] AdditionalVRSystems => (from system in additionalSystems where system.AdditionalSystemType == AdditionalSystemTypes.VR select system.Prefab).ToArray() ?? null;

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Destroys all listed components.
        /// </summary>
        /// <param name="ForceRemoveComponents">If true, retries to destroy components that weren't destroyed previously (because of <see cref="RequireComponent"/> or any other similar behaviour) until all are destroyed.</param>
        public void RemoveComponents() {
            if (shouldSkip) //Debug
                return;

            //Destroying transforms is not allowed in Unity, we'll destroy the GameObjects instead.
            DestroyGameobjectsFromTransforms();
            DestroyComponents();
        }

        #endregion

        #region PRIVATE METHODS

        private void DestroyGameobjectsFromTransforms() {
            foreach (var obj in gameObjectsToDestroy) {
                if (!destroyGameobjectsComponents)
                    Destroy(obj);
                else {
                    var components = obj.GetComponents<Component>().Where(component => component is not Transform);
                    int maxRetries = 100;
                    int currentRetries = 0;
                    bool canExit = false;
                    do {
                        currentRetries++;
                        if (currentRetries > maxRetries) {
                            canExit = true;
                        }
                        components.ForEach(component => { try { DestroyImmediate(component); } catch {} });
                        components = components.Where(component => component != null);
                    } while (!canExit);
                }
            }

            //Old method that tried to understand if the component is a transform then tries to destroy it; 
            //Worked just fine on PC and Android phones, but gave enumeration problems on Quest 1 and Quest 2.
            //Probably some kind of reflection problem when trying to retrieve the type of the component
            ////Get the transforms
            //var transforms = from component in componentsToRemove
            //                 where component.GetType() == typeof(Transform)
            //                 select component;
            //
            ////Remove the transforms from the original list
            //var nonTransformComponents = from component in componentsToRemove
            //                     where component.GetType() != typeof(Transform)
            //                     select component;
            //componentsToRemove = nonTransformComponents.ToArray();
            //
            ////Destroy the gameobjects associated to the transforms
            //foreach (var transform in transforms) {
            //    Log.D($"Destroying transform GameObject {transform.name}");
            //    Destroy(transform.gameObject);
            //}
        }

        private void DestroyComponents() {
            //We use a coroutine so that a frame can pass after issuing the Destroy command, otherwise we'd have to use DestroyImmediate, which can have weird consequences.
            StartCoroutine(RemoveComponentsMultiPassCoroutine());
        }

        /// <summary>
        /// Tries repeatedly to destroy listed components; this because components might have dependencies that do not allow them to be removed, so we try to destroy other components first before trying again, so maybe we'll remove the dependency first.
        /// </summary>
        private IEnumerator RemoveComponentsMultiPassCoroutine() {
            var components = new List<Component>(componentsToRemove);
            var remainingComponents = new List<Component>();
            bool allDestroyed = false;
            int tries = 0;
            var waitForEndOfFrame = new WaitForEndOfFrame();
            do {
                foreach (var component in components) {
                    //Skip already destroyed, missing or duplicated references to components
                    if (component == null)
                        continue;
                    //Try to destroy the component
                    try {
                        Destroy(component);
                    } catch {
                        remainingComponents.Add(component);
                    }
                }
                components = new List<Component>(remainingComponents);
                remainingComponents.Clear();
                tries++;
                yield return waitForEndOfFrame;
                allDestroyed = (components.Count > 0) ? false : true;
            } while (!allDestroyed && tries < maxRetries);

            if (!allDestroyed) {
                var names = (from component in components
                             select component.GetType().FullName)
                            .ToList();
                var concatenatedNames = string.Join(", ", names);
                Log.E($"Some components could not be removed from the Player. Components list: {concatenatedNames}");
            }
        }

        #endregion

        #region EDITOR UTILITIES
        //TODO maybe move this stuff in custom inspector?

        [ContextMenu("Reorder by RequireComponent")]
        private void ReorderByRequireComponent() {
            var constrainedComponents = from component in componentsToRemove
                                        where component.GetType().IsDefined(typeof(RequireComponent))
                                        select component;
            var unconstrainedComponents = from component in componentsToRemove
                                          where !component.GetType().IsDefined(typeof(RequireComponent))
                                          select component;
            var result = new List<Component>();
            result.AddRange(constrainedComponents);
            result.AddRange(unconstrainedComponents);
            componentsToRemove = result.ToArray();
        }

        [ContextMenu("Remove components already covered by GameObject's destruction")]
        private void RemoveComponentsAlreadyInGameObjectDestruction() {
            var result = new List<Component>();
            bool shouldSkip = false;
            foreach (var component in componentsToRemove) {
                if (component == null)
                    continue;
                shouldSkip = false;
                foreach (var obj in gameObjectsToDestroy) {
                    if (obj == null)
                        continue;
                    if (component.gameObject == obj) {
                        shouldSkip = true;
                        break;
                    }
                }
                if (!shouldSkip) {
                    result.Add(component);
                }
            }

            componentsToRemove = result.ToArray();
        }

        [ExposeInInspector]
        private void AddToComponentsToDeleteAllComponentsOfType(string assemblyName, string className) {
            var typeToSearch = Type.GetType(className) ??
                Type.GetType($"{assemblyName}.{className}, {assemblyName}");
            var components = this.gameObject.GetComponentsInChildren(typeToSearch);
            var componentsList = componentsToRemove.ToList(); //HACK: dumb array to list to array conversion because I decided to make the components to remove an array >.> but it's okay because this will always only execute in Editor.
            componentsList.AddRange(components);
            componentsToRemove = componentsList.ToArray();
        }
        #endregion

    }

}
