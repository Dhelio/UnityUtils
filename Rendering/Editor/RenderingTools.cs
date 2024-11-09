using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using Castrimaris.Core.Extensions;
using UnityEngine;
using Castrimaris.Rendering;
using Castrimaris.Core.Monitoring;

namespace Castrimaris.Core.Editor {

    public class RenderingTools {
        [MenuItem("Tools/Rendering/Add Dynamic Lightmaps to Static Objects")]
        public static void AddDynamicLightmapsToAllStaticObjects() {
            int counter = 0;
            var objs = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            var staticObjs = from obj in objs
                             where obj.GetComponent<MeshRenderer>() != null && obj.isStatic
                             select obj;
            foreach (var obj in staticObjs) {
                obj.AddComponent<DynamicLightmap>();
                counter++;
            }

            Log.I($"Added {nameof(DynamicLightmap)} to {counter} objects");
        }


    }
}