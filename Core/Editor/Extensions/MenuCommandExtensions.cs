using Castrimaris.Core.Monitoring;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Core.Editor.Extensions {

    public static class MenuCommandExtensions {

        /// <summary>
        /// Checks if the command is being executed for the first time in the selection
        /// </summary>
        /// <returns>>True if it's the first time the command is being executed for the selected objects, false otherwise.</returns>
        public static bool CheckFirstTimeForSelection(this MenuCommand command) {
            if (Selection.objects.Length <= 1) {
                Log.E("Tried to run command only once on a single or null object! Please, select more GameObjects.");
                return false;
            }

            //Check if current context object is the first in the selection, otherwise behaviour would be replicated for each object in the selection.
            if (command.context != Selection.objects[0])
                return false;

            return true;
        }
        
    }

}