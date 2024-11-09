using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Castrimaris.Core.Editor.Extensions;
using System.Linq;
using Castrimaris.Math;

namespace Castrimaris.Core.Editor {

    /// <summary>
    /// Misc tools for Hierarchy's menu item.
    /// </summary>
    public class MenuItemTools {

        /// <summary>
        /// Groups multiple gameobject under a parent gameobject.
        /// </summary>
        [MenuItem("GameObject/Castrimaris/Group")]
        private static void GroupGameobjects(MenuCommand command) {
            if (Selection.count == 1) {
                var parentObj = new GameObject("Parent");
                parentObj.transform.parent = ((GameObject)command.context).transform.parent;
                ((GameObject)command.context).transform.SetParent(parentObj.transform);
                return;
            }

            if (!command.CheckFirstTimeForSelection())
                return;

            var target = (GameObject)command.context;

            GameObject parent = new GameObject("Parent");
            parent.transform.parent = target.transform.parent;

            GameObjectUtility.SetParentAndAlign(parent, target.transform.parent.gameObject);
            foreach (var obj in Selection.objects) {
                var gameObj = obj as GameObject;
                gameObj.transform.SetParent(parent.transform);
            }

            Selection.activeObject = parent;
        }

        /// <summary>
        /// Groups multiple gameobject under a parent <see cref="GameObject"/>, renaming the parent <see cref="GameObject"/> 
        /// with the common name the objects have.
        /// E.g. if the objects are called "Hair_1", "Hair_2", etc then the parent will be named "Hair_"
        /// </summary>
        [MenuItem("GameObject/Castrimaris/Group With Naming")]
        private static void GroupGameObjectWithNaming(MenuCommand command) {
            if (!command.CheckFirstTimeForSelection())
                return;

            //Find common name
            string commonName = "";
            var firstGameObj = Selection.objects.First() as GameObject;
            for (int i = 0; i < firstGameObj.name.Length; i++) {
                bool shouldQuit = false;
                for (int k = 1; k < Selection.objects.Length; k++) {
                    var examinedGameObj = Selection.objects[k] as GameObject;
                    if (firstGameObj.name[i] != examinedGameObj.name[i]) { 
                        shouldQuit = true;
                        break;
                    }
                }
                if (!shouldQuit) {
                    commonName += firstGameObj.name[i];
                } else {
                    break;
                }
            }

            //Group
            var target = (GameObject)command.context;

            GameObject parent = new GameObject(commonName);
            parent.transform.parent = target.transform.parent;

            GameObjectUtility.SetParentAndAlign(parent, target.transform.parent.gameObject);
            foreach (var obj in Selection.objects) {
                var gameObj = obj as GameObject;
                gameObj.transform.SetParent(parent.transform);
            }

            Selection.activeObject = parent;
        }

        /// <summary>
        /// Groups all selected gameobjects by name similarity. That means that similar names get grouped under the commnon name (ex. "eyes" and "eye" get grouped under a new gameobject "eye")
        /// </summary>
        [MenuItem("GameObject/Castrimaris/Group Selection by Name Similarity")]
        private static void GroupGameObjectBySimilarName(MenuCommand command) {
            if (!command.CheckFirstTimeForSelection())
                return;

            int maxDifference = 2;

            var gameObjs = Selection.gameObjects.OrderBy(gameObject => gameObject.name).ToList();
            var groupedObjects = new List<List<GameObject>>();

            for (int i = 0, k = 0, n = 0; i < gameObjs.Count; n++) {
                groupedObjects.Add(new List<GameObject>());
                groupedObjects[n].Add(gameObjs[i]);
                for (k = i + 1; k < gameObjs.Count; k++) {
                    if (FMath.LevenshteinDistance(gameObjs[i].name, gameObjs[k].name) <= maxDifference) {
                        groupedObjects[n].Add(gameObjs[k]);
                    } else {
                        break;
                    }
                }
                i = i + (k - i);
            }

            foreach (var group in groupedObjects) {
                var parent = new GameObject();
                if (group.Count <= 1) {
                    parent.name = group[0].name;
                } else {
                    parent.name = group[0].name.Substring(0, group[0].name.Length - FMath.LevenshteinDistance(group[0].name, group[1].name));
                }
                GameObjectUtility.SetParentAndAlign(parent, group[0].transform.parent.gameObject);
                foreach (var obj in group) {
                    obj.transform.SetParent(parent.transform);
                }
            }
        }

        /// <summary>
        /// Copies all components from a <see cref="GameObject"/> to another.
        /// </summary>
        [MenuItem("GameObject/Castrimaris/Copy Components")]
        private static void CopyComponents(MenuCommand command) {
            if (!command.CheckFirstTimeForSelection())
                return;

            var source = Selection.objects[0] as GameObject;
            var sourceComponents = source.GetComponents<Component>();
            foreach (var component in sourceComponents) {
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
                for (int i = 1; i < Selection.objects.Length; i++) {
                    var target = Selection.objects[i] as GameObject;
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
                }
            }
        }

        /// <summary>
        /// Copies all components from a <see cref="GameObject"/> to another recursively
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("GameObject/Castrimaris/Recursive Copy Components")]
        private static void RecursiveCopyComponents(MenuCommand command) {
            if (!command.CheckFirstTimeForSelection())
                return;

            if (Selection.objects.Length != 2) {
                Debug.LogError($"[{nameof(RecursiveCopyComponents)}] - This method can only be called on exactly two objects! Please, select only two objects in the hierarchy");
                return;
            }

            var source = Selection.objects[0] as GameObject;
            var target = Selection.objects[1] as GameObject;

            var srcTransforms = source.GetComponentsInChildren<Transform>();
            var trtTransforms = target.GetComponentsInChildren<Transform>();

            if (srcTransforms.Length != trtTransforms.Length) {
                Debug.LogError($"[{nameof(RecursiveCopyComponents)}] - Target and Source child count differs! Recursive copy can only be done on similar objects!");
                return;
            }

            RecursiveCopyComponents(source, target);
        }

        /// <summary>
        /// Sorts the <see cref="GameObject"/>s by name
        /// </summary>
        [MenuItem("GameObject/Castrimaris/Sort by Name")]
        private static void SortByName(MenuCommand command) {
            if (!command.CheckFirstTimeForSelection())
                return;

            var parentNames = (from obj in Selection.objects
                               select (obj as GameObject).transform.parent.name).Distinct().ToList();
            if (parentNames.Count > 1) {
                Debug.LogError($"[{nameof(SortByName)}] - Error! Cannot sort objects from different parents!");
                return;
            }

            var target = (Selection.objects[0] as GameObject).transform.parent.gameObject;

            //TODO fix, should only order the selection
            var cmd = new MenuCommand(target);
            SortChildrenByName(cmd);
        }

        /// <summary>
        /// Sorts this <see cref="GameObject"/> children by name
        /// </summary>
        [MenuItem("GameObject/Castrimaris/Sort Children by Name")]
        private static void SortChildrenByName(MenuCommand command) {
            var target = (GameObject)command.context;

            var names = new List<string>();
            for (int i = 0; i < target.transform.childCount; i++) {
                names.Add(target.transform.GetChild(i).name);
            }

            names = names.OrderBy(x => x).ToList();

            for (int i = 0; i < names.Count; i++) {
                target.transform.Find(names[i]).SetSiblingIndex(i);
            }
        }

        /// <summary>
        /// Adds a <see cref="MeshCollider"/> to all <see cref="MeshRenderer"/>s in this object and its children.
        /// </summary>
        [MenuItem("GameObject/Castrimaris/Add Mesh Collider to all MeshRenderers in this object and its children")]
        private static void AddMeshColliderToAllMeshRenderers(MenuCommand command) {
            if (Selection.count > 1) {
                Debug.LogError($"[{nameof(AddMeshColliderToAllMeshRenderers)}] - Tried to run this command on multiple selection. Only 1 object is allowed.");
                return;
            }

            var target = (GameObject)command.context;

            var meshRenderers = target.GetComponentsInChildren<MeshRenderer>();
            uint addedMeshCollidersCount = 0;
            foreach (var meshRenderer in meshRenderers) {
                if (meshRenderer.GetComponent<MeshCollider>())
                    continue;
                meshRenderer.gameObject.AddComponent<MeshCollider>();
                addedMeshCollidersCount++;
            }
            Debug.Log($"[{nameof(AddMeshColliderToAllMeshRenderers)}] - Operation terminated. Added {addedMeshCollidersCount} mesh colliders.");
        }

        private static void RecursiveCopyComponents(GameObject Source, GameObject Target) {
            if (Source.transform.childCount > 0) {
                for (int i = 0; i < Source.transform.childCount; i++) {
                    RecursiveCopyComponents(Source.transform.GetChild(i).gameObject, Target.transform.GetChild(i).gameObject);
                }
            }
            var sourceComponents = Source.GetComponents<Component>();
            foreach (var component in sourceComponents) {
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(Target);
            }
        }
    }
}
