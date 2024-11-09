using Castrimaris.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Core.Collections {

    /// <summary>
    /// Custom drawer for <see cref="SerializableTimeSpan"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableTimeSpan))]
    public class SerializableTimeSpanDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Start property drawing
            EditorGUI.BeginProperty(position, label, property);

            // Calculate the position and size for each field
            var labelWidth = EditorGUIUtility.labelWidth;
            var fieldWidth = (position.width - labelWidth) / 4;

            // Draw the label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Save the original indent level
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects for each field
            var daysRect = new Rect(position.x, position.y, fieldWidth, position.height);
            var hoursRect = new Rect(position.x + fieldWidth, position.y, fieldWidth, position.height);
            var minutesRect = new Rect(position.x + fieldWidth * 2, position.y, fieldWidth, position.height);
            var secondsRect = new Rect(position.x + fieldWidth * 3, position.y, fieldWidth, position.height);

            // Get properties
            var daysProp = property.FindPropertyRelative("Days");
            var hoursProp = property.FindPropertyRelative("Hours");
            var minutesProp = property.FindPropertyRelative("Minutes");
            var secondsProp = property.FindPropertyRelative("Seconds");

            // Draw fields
            daysProp.intValue = EditorGUI.IntField(daysRect, daysProp.intValue);
            hoursProp.intValue = EditorGUI.IntField(hoursRect, hoursProp.intValue);
            minutesProp.intValue = EditorGUI.IntField(minutesRect, minutesProp.intValue);
            secondsProp.intValue = EditorGUI.IntField(secondsRect, secondsProp.intValue);

            // Restore the indent level
            EditorGUI.indentLevel = indentLevel;

            // Synchronize the TimeSpanValue with the serialized fields
            var serializableTimeSpan = fieldInfo.GetValue(property.serializedObject.targetObject) as SerializableTimeSpan;
            serializableTimeSpan?.Synchronize();

            // End property drawing
            EditorGUI.EndProperty();
        }
    }
}