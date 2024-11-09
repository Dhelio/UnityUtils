
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Core.Collections {

    /// <summary>
    /// Custom drawer for <see cref="SerializableDateTime"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableDateTime))]
    public class SerializableDateTimeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Start property drawing
            EditorGUI.BeginProperty(position, label, property);

            // Draw the label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Save the original indent level and set it to 0
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects for each label and field
            float labelWidth = 15f;
            float fieldWidth = (position.width - 6 * labelWidth) / 6;
            float padding = 2f;

            Rect yearLabelRect = new Rect(position.x, position.y, labelWidth, position.height);
            Rect yearFieldRect = new Rect(position.x + labelWidth, position.y, fieldWidth - padding, position.height);

            Rect monthLabelRect = new Rect(position.x + labelWidth + fieldWidth, position.y, labelWidth, position.height);
            Rect monthFieldRect = new Rect(position.x + 2 * labelWidth + fieldWidth, position.y, fieldWidth - padding, position.height);

            Rect dayLabelRect = new Rect(position.x + 2 * (labelWidth + fieldWidth), position.y, labelWidth, position.height);
            Rect dayFieldRect = new Rect(position.x + 3 * labelWidth + 2 * fieldWidth, position.y, fieldWidth - padding, position.height);

            Rect hourLabelRect = new Rect(position.x + 3 * (labelWidth + fieldWidth), position.y, labelWidth, position.height);
            Rect hourFieldRect = new Rect(position.x + 4 * labelWidth + 3 * fieldWidth, position.y, fieldWidth - padding, position.height);

            Rect minuteLabelRect = new Rect(position.x + 4 * (labelWidth + fieldWidth), position.y, labelWidth, position.height);
            Rect minuteFieldRect = new Rect(position.x + 5 * labelWidth + 4 * fieldWidth, position.y, fieldWidth - padding, position.height);

            Rect secondLabelRect = new Rect(position.x + 5 * (labelWidth + fieldWidth), position.y, labelWidth, position.height);
            Rect secondFieldRect = new Rect(position.x + 6 * labelWidth + 5 * fieldWidth, position.y, fieldWidth - padding, position.height);

            // Get properties
            SerializedProperty yearProp = property.FindPropertyRelative("year");
            SerializedProperty monthProp = property.FindPropertyRelative("month");
            SerializedProperty dayProp = property.FindPropertyRelative("day");
            SerializedProperty hourProp = property.FindPropertyRelative("hour");
            SerializedProperty minuteProp = property.FindPropertyRelative("minute");
            SerializedProperty secondProp = property.FindPropertyRelative("second");

            // Draw labels and fields
            EditorGUI.LabelField(yearLabelRect, "Y");
            yearProp.intValue = EditorGUI.IntField(yearFieldRect, yearProp.intValue);

            EditorGUI.LabelField(monthLabelRect, "M");
            monthProp.intValue = EditorGUI.IntField(monthFieldRect, monthProp.intValue);

            EditorGUI.LabelField(dayLabelRect, "D");
            dayProp.intValue = EditorGUI.IntField(dayFieldRect, dayProp.intValue);

            EditorGUI.LabelField(hourLabelRect, "H");
            hourProp.intValue = EditorGUI.IntField(hourFieldRect, hourProp.intValue);

            EditorGUI.LabelField(minuteLabelRect, "m");
            minuteProp.intValue = EditorGUI.IntField(minuteFieldRect, minuteProp.intValue);

            EditorGUI.LabelField(secondLabelRect, "S");
            secondProp.intValue = EditorGUI.IntField(secondFieldRect, secondProp.intValue);

            // Restore the indent level
            EditorGUI.indentLevel = indentLevel;

            // End property drawing
            EditorGUI.EndProperty();
        }
    }
}