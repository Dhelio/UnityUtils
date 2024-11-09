using Castrimaris.Core.Monitoring;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Attributes {

    /// <summary>
    /// Custom drawer that implements the behaviour for the attribute <see cref="RequireInterfaceAttribute"/>.
    /// Checks if the assigned GameObjects have the required interface.
    /// </summary>
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            //Sanity Checks
            if (property.propertyType != SerializedPropertyType.ObjectReference) {
                Log.E($"Could not serialize property {property.name}! To be serialized correctly the property must be an Object Reference.");
                return;
            }

            if (property.type != "PPtr<$GameObject>") {
                Log.E($"RequireInterface property can only be used on GameObjects!");
                return;
            }

            //Get attribute data
            var serializeInterfaceAttribute = attribute as RequireInterfaceAttribute;

            //Drag and Drop behaviour
            if (DragAndDrop.objectReferences.Length > 0) {
                var firstGO = DragAndDrop.objectReferences[0] as GameObject;
                if (firstGO == null) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    Log.E($"Cannot assign a reference that is not a GameObject reference; don't drag the script, drag the object!");
                    return;
                }
                //Log.D($"Trying to get component of type {serializeInterfaceAttribute.Type} from {firstGO.name}");
                if (firstGO.GetComponent(serializeInterfaceAttribute.Type) == null) {
                    //Refuse the object, we are only looking for those that contain the interface!!
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    Log.E($"Could not assign item {firstGO.name} to the field because it does not have the interface {serializeInterfaceAttribute.Type}");
                    return;
                }
            }

            //Object picker behaviour
            if (property.objectReferenceValue != null) {
                // Check if the interface is present.
                GameObject go = property.objectReferenceValue as GameObject;
                if (go != null && go.GetComponent(serializeInterfaceAttribute.Type) == null) {
                    //Refuse the object, we are only looking for those that contain the interface!!
                    property.objectReferenceValue = null;
                    Log.E($"Could not assign item {go.name} to the field because it does not have the interface {serializeInterfaceAttribute.Type}");
                }
            }

            property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(GameObject), true);
        }
    }

}