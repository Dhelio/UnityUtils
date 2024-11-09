using UnityEditor;

namespace Castrimaris.Physics {

    /// <summary>
    /// Custom Editor for the original PhysicsCollisionButton class
    /// </summary>
    [CustomEditor(typeof(PhysicsCollisionButton))]
    public class PhysicsButtonInspector : Editor {

        //private SerializedProperty activationPointProperty; //the original property in the original class
        //private Vector3 activationPoint; //support data
        //
        //#region UNITY OVERRIDES
        //
        //private void OnEnable() {
        //    activationPointProperty = serializedObject.FindProperty("activationPoint");
        //    activationPoint = activationPointProperty.vector3Value;
        //}
        //
        //public override VisualElement CreateInspectorGUI() {
        //    VisualElement gui = new VisualElement();
        //
        //    InspectorElement.FillDefaultInspector(gui, serializedObject, this);
        //
        //    gui.AddButton("Save Activation Point Position", () => {
        //        activationPoint = ((PhysicsCollisionButton)target).transform.localPosition;
        //        activationPointProperty.vector3Value = activationPoint;
        //        serializedObject.ApplyModifiedProperties();
        //    });
        //
        //    return gui;
        //}
        //
        //#endregion
    }
}