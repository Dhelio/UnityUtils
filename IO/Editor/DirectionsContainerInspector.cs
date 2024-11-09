using Castrimaris.Core.Editor;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.GoogleStreetView;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.IO.GoogleDirections {

    [CustomEditor(typeof(DirectionsContainer))]
    public class DirectionsContainerInspector : Editor {

        private DirectionsContainer directionsContainer;

        private void OnEnable() {
            directionsContainer = target as DirectionsContainer;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            Layout.Button("Save Directions StreetViews to Disk", SaveDirectionsStreetViewsToDisk);
        }

        /// <summary>
        /// Editor method used to save the 360 views of the various locations in the direction container
        /// </summary>
        private async void SaveDirectionsStreetViewsToDisk() {
            var directions = directionsContainer.Data;
            var routes = directions.Routes.First();
            var legs = routes.Legs.First();
            var steps = legs.Steps;
            var locations = (from step in steps
                             select step.StartLocation).ToArray();
            int index = 0;
            foreach (var location in locations) {
                Texture2D image = null;
                try {
                    image = await StreetView.RequestFullEquirectangularView(new EquirectangularViewRequest(
                        Latitude: location.Latitude,
                        Longitude: location.Longitude
                        ));
                } catch (StreetViewException _) {
                    Log.E($"Could not download equirectangular view for location {location.Latitude},{location.Longitude}. Skipping.");
                    continue;
                }
                var bytes = image.EncodeToJPG(); //We're encoding it to jpg to save some space
                var directoryName = $"{Application.dataPath}\\Resources\\StreetView\\";
                var fileName = $"{this.name}_{index}.jpg";
                if (!System.IO.Directory.Exists(directoryName))
                    System.IO.Directory.CreateDirectory(directoryName);
                System.IO.File.WriteAllBytes(directoryName + fileName, bytes);
                index++;
            }

            AssetDatabase.Refresh();
        }

    }

}