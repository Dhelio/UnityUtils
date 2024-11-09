using Castrimaris.Attributes;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.ScriptableObjects {

    /// <summary>
    /// Scriptable object that contains the scene names of the build settings.
    /// Useful to execute particular build settings in the Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "Scene Names", menuName = "Castrimaris/ScriptableObjects/Scene Names")]
    public class SceneNames : ScriptableObject {

        [ReadOnly] public string[] fullNames;
        [ReadOnly] public string[] shortNames;

#if UNITY_EDITOR
        public void RetrieveSceneNames() {
            if (!Application.isEditor) {
                return;
            }

            //Retrieve active scenes in the build settings
            var activeScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();

            //Retrieve the full path names (e.g. "Assets/Scenes/MyScene.unity")
            var activeScenesPaths = from scene in activeScenes
                                    select scene.path;

            fullNames = activeScenesPaths.ToArray();

            //Take only the name of the scene.
            var shortenedNames = from name in fullNames
                                 select name.Split("/").Last().Remove((name.Split("/").Last().Length) - (".unity".Length));

            shortNames = shortenedNames.ToArray();
        }

        [MenuItem("Tools/Castrimaris/Update Scene Names Scriptable Object")]
        public static void UpdateScriptables() {
            var assetsGuids = AssetDatabase.FindAssets("t: SceneNames", null);
            foreach ( var assetGuid in assetsGuids ) { //TODO: this method can be generalized for most Editor tools!
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(SceneNames)) as SceneNames;
                asset.RetrieveSceneNames();
            }
        }
#endif

    }

}
