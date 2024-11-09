using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.GoogleDirections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO.GoogleStreetView {

    public class StreetViewSphereVisualizer : MonoBehaviour {

        #region Private Variables

        private const string RESOURCES_DIRECTORY = "Streetview\\";

        [Header("References")]
        [SerializeField] private MeshRenderer sphere;
        [SerializeField] private DirectionsContainer currentActiveContainer = null;

        [Header("Events")]
        [SerializeField] private UnityEvent onViewsLoaded = new UnityEvent();

        private List<Texture2D> views = new List<Texture2D>();
        private string activeViewName = string.Empty;

        #endregion

        #region Properties

        public int Count => views.Count;
        public DirectionsContainer CurrentActiveContainer => currentActiveContainer;
        public string ActiveViewDescription => activeViewName.Split("_").Last();

        #endregion

        //TODO save and load these views from persistent data path
        public void LoadViews(DirectionsContainer directionsContainer) {
            currentActiveContainer = directionsContainer;
            Log.D($"Loading views for directions for {directionsContainer.name} from Resources.");
            var allViews = Resources.LoadAll<Texture2D>(RESOURCES_DIRECTORY);
            var views = (from view in allViews
                         where view.name.StartsWith(directionsContainer.name)
                         select view).ToList();
            var unusedViews = (from view in allViews
                               where !view.name.StartsWith(directionsContainer.name)
                               select view).ToList();
            for (int i = 0; i < unusedViews.Count(); i++) {
                Resources.UnloadAsset(unusedViews[i]);
            }
            this.views = views;
            Log.D($"Loading done. Loaded {views.Count} views.");
            SetActiveView(0);
            onViewsLoaded.Invoke();
        }

        public void SetActiveView(int index) {
            sphere.sharedMaterial.mainTexture = views[index];
            activeViewName = views[index].name;
        }

        //TODO move this to new input system, otherwise there will be problems
        //private int index = 0;
        //public void Update() {
        //    if (Input.GetKeyDown(KeyCode.RightArrow)) {
        //        index = Mathf.Clamp(index + 1, 0, views.Count - 1);
        //        SetActiveView(index);
        //    } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
        //        index = Mathf.Clamp(index - 1, 0, views.Count - 1);
        //        SetActiveView(index);
        //    }
        //}
    }
}