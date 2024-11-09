using Castrimaris.Core.Monitoring;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Attributes {

    /// <summary>
    /// Draws a property only if certain criteria are met.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
    public class ConditionalFieldDrawer : PropertyDrawer {

        private ConditionalFieldAttribute conditionalField;
        private SerializedProperty targetProperty;
        private bool shouldBeDrawn = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            RetrieveData(property);
            CheckIfShouldBeDrawn();
            DrawField(position, property);
        }

        /// <summary>
        /// If the data it is checking is equal to what it's expecting, then draws the field. Otherwise, hide it.
        /// </summary>
        private void CheckIfShouldBeDrawn() {
            //New kind of variables can be added in the switch/case!
            switch (targetProperty.type) {
                case "bool":
                    shouldBeDrawn = targetProperty.boolValue.Equals(conditionalField.TargetPropertyValue);
                    break;
                case "Enum":
                    shouldBeDrawn = targetProperty.enumValueIndex.Equals((int)conditionalField.TargetPropertyValue);
                    break;
                case "int":
                    shouldBeDrawn = targetProperty.intValue.Equals((int)conditionalField.TargetPropertyValue);
                    break;
                case "uint":
                    shouldBeDrawn = targetProperty.uintValue.Equals((uint)conditionalField.TargetPropertyValue);
                    break;
                case "ulong":
                    shouldBeDrawn = targetProperty.ulongValue.Equals((ulong)conditionalField.TargetPropertyValue);
                    break;
                case "long":
                    shouldBeDrawn = targetProperty.longValue.Equals((long)conditionalField.TargetPropertyValue);
                    break;
                case "float":
                    shouldBeDrawn = targetProperty.floatValue.Equals((float)conditionalField.TargetPropertyValue);
                    break;
                case "double":
                    shouldBeDrawn = targetProperty.doubleValue.Equals((double)conditionalField.TargetPropertyValue);
                    break;
                case "String":
                case "string":
                    shouldBeDrawn = targetProperty.stringValue.Equals(conditionalField.TargetPropertyValue.ToString());
                    break;
                default:
                    Log.E($"No behaviour defined for property of type {targetProperty.type}! Showing variable...");
                    shouldBeDrawn = true;
                    break;
            }
        }

        private void RetrieveData(SerializedProperty property) {
            conditionalField = attribute as ConditionalFieldAttribute;
            targetProperty = property.serializedObject.FindProperty(conditionalField.TargetPropertyName);
            if (targetProperty == null) {
                Log.E($"Cannot find property with name: {conditionalField.TargetPropertyName}");
            }
        }

        private void DrawField(Rect position, SerializedProperty property) {
            if (shouldBeDrawn) {
                EditorGUI.PropertyField(position, property);
            } else if (conditionalField.DisablingType == DisablingTypes.ReadOnly) {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property);
                GUI.enabled = true;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!shouldBeDrawn && conditionalField.DisablingType == DisablingTypes.Hidden)
                return 0f;
            return base.GetPropertyHeight(property, label);
        }

    }

}