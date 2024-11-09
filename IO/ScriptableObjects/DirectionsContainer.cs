using Castrimaris.IO.GoogleDataStructures;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.IO.GoogleDirections {

    [CreateAssetMenu(fileName = "Directions", menuName = "Castrimaris/ScriptableObjects/Google Directions")]
    public class DirectionsContainer : ScriptableObject {

        [Header("Parameters")]
        public DirectionsResponse Data;

        public string Instructions {
            get {
                if (Data == null)
                    return "No data available for the instructions of the requested directions";

                var directions = Data;
                var routes = directions.Routes.First();
                var legs = routes.Legs.First();
                var steps = legs.Steps;
                var htmlInstructions = (from step in steps
                                        select step.HtmlInstructions).ToArray();
                var instructions = string.Join("\n", htmlInstructions);
                return instructions;
            }
        }

        public List<Step> Steps {
            get {
                if (Data == null)
                    return null;
                var directions = Data;
                var routes = directions.Routes.First();
                var legs = routes.Legs.First();
                var steps = legs.Steps;

                return steps;
            }
        }

        public List<Location> Locations {
            get {
                var locations = new List<Location>();
                var steps = Steps;
                locations.Add(steps.First().StartLocation);
                foreach (var step in steps) {
                    locations.Add(step.EndLocation);
                }
                return locations;
            }
        }
    }

}