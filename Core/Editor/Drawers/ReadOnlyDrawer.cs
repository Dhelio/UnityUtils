using UnityEditor;
using UnityEngine;

namespace Castrimaris.Attributes {

    /// <summary>
    /// If a field or property is tagged with <see cref="ReadOnlyAttribute"/> then draws it as non interactable
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            // Saving previous GUI enabled value
            var previousGUIState = GUI.enabled;
            // Disabling edit for property
            GUI.enabled = false;
            // Drawing Property
            EditorGUI.PropertyField(position, property, label, true);
            // Setting old GUI enabled value
            GUI.enabled = previousGUIState;
        }
    }
}