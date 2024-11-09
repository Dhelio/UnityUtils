using System;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Core.Editor {

    /// <summary>
    /// Utility class for building quickly Editor inspectors or utilities.
    /// </summary>
    public static class Layout {

        /// <summary>
        /// Makes a field using <see cref="EditorGUILayout.Vector3Field(string, Vector3, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="Label">The name of the field</param>
        /// <param name="ValueReference">The reference of the ValueReference to use for the field.</param>
        public static void Vector3Field(string Label, ref Vector3 ValueReference) {
            ValueReference = EditorGUILayout.Vector3Field(Label, ValueReference);
        }

        public static Vector3 Vector3Field(string Label, Vector3 ValueReference) {
            return EditorGUILayout.Vector3Field(Label, ValueReference);
        }

        /// <summary>
        /// Makes a field using <see cref="EditorGUILayout.Toggle(string, bool, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="Label">The name of the field</param>
        /// <param name="ValueReference">The reference of the ValueReference to use for the field.</param>
        public static void Toggle(string Label, ref bool ValueReference) {
            ValueReference = EditorGUILayout.Toggle(Label, ValueReference);
        }

        /// <summary>
        /// Makes a field using <see cref="EditorGUILayout.IntField(string, int, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="Label">The name of the field</param>
        /// <param name="ValueReference">The reference of the ValueReference to use for the field.</param>
        public static void IntField(string Label, ref int ValueReference) {
            ValueReference = EditorGUILayout.IntField(Label, ValueReference);
        }

        /// <inheritdoc cref="Toggle(string, ref bool)"/>
        public static void BoolField(string Label, ref bool ValueReference) {
            Toggle(Label, ref ValueReference);
        }

        /// <summary>
        /// Makes a field using <see cref="EditorGUILayout.FloatField(string, float, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="Label"></param>
        /// <param name="ValueReference"></param>
        public static void FloatField(string Label, ref float ValueReference) {
            ValueReference = EditorGUILayout.FloatField(Label, ValueReference);
        }

        /// <summary>
        /// Makes a field using <see cref="EditorGUILayout.EnumPopup(Enum, GUILayoutOption[])"/>.
        /// </summary>
        /// <typeparam name="T">An enum of type <see cref="Enum"/></typeparam>
        /// <param name="Label">The name of the field</param>
        /// <param name="ValueReference">The reference of the ValueReference to use for the field.</param>
        public static void EnumField<T>(string Label, ref T ValueReference) where T : Enum {
            ValueReference = (T)EditorGUILayout.EnumPopup(Label, ValueReference);
        }

        /// <summary>
        /// Makes a field using <see cref="EditorGUILayout.ObjectField(string, UnityEngine.Object, Type, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="Label">The name of the field</param>
        /// <param name="ValueReference">The reference of the ValueReference to use for the field.</param>
        /// <param name="allowSceneObjects">Wheter the user can search in scene objects.</param>
        public static void ObjectField(string Label, ref GameObject ValueReference, bool allowSceneObjects = true) {
            ValueReference = EditorGUILayout.ObjectField(Label, ValueReference, typeof(GameObject), allowSceneObjects) as GameObject;
        }

        /// <summary>
        /// Makes a field using <see cref="EditorGUILayout.ObjectField(string, UnityEngine.Object, Type, GUILayoutOption[])"/>.
        /// </summary>
        /// <typeparam name="T">A component that inherits from <see cref="UnityEngine.Object"/></typeparam>
        /// <param name="label">The name of the field</param>
        /// <param name="valueReference">The reference of the ValueReference to use for the field.</param>
        /// <param name="allowSceneObjects">Wheter the user can search in scene objects.</param>
        public static void ObjectField<T>(string label, ref T valueReference, bool allowSceneObjects = true) where T : UnityEngine.Object {
            valueReference = EditorGUILayout.ObjectField(label, valueReference, typeof(T), allowSceneObjects) as T;
        }

        //TODO docs
        public static void TextField(string label, ref string valueReference) {
            valueReference = EditorGUILayout.TextField(label, valueReference);
        }

        //TODO docs
        public static void ObjectArray<T>(string Label, ref T[] ValueReference) where T : UnityEngine.Object {
            LabelField(Label);
            for (int i = 0; i < ValueReference.Length; i++) {
                ValueReference[i] = EditorGUILayout.ObjectField($"Item {i}", ValueReference[i], typeof(T), false) as T;
            }
        }

        //TODO docs
        public static void ScriptablesArray<T>(string Label, ref T[] ValueReference) where T : UnityEngine.ScriptableObject {
            LabelField(Label);
            for (int i = 0; i < ValueReference.Length; i++) {
                ValueReference[i] = EditorGUILayout.ObjectField($"Item {i}", ValueReference[i], typeof(T), false) as T;
            }
        }

        /// <summary>
        /// Makes a ReadOnly field using <see cref="EditorGUILayout.ObjectField(string, UnityEngine.Object, Type, GUILayoutOption[])"/>.
        /// </summary>
        /// <typeparam name="T">A component that inherits from <see cref="UnityEngine.Object"/></typeparam>
        /// <param name="Label">The name of the field</param>
        /// <param name="ValueReference">The reference of the ValueReference to use for the field.</param>
        /// <param name="AllowSceneObjects">Wheter the user can search in scene objects.</param>
        public static void ReadOnlyObjectField<T>(string Label, ref T ValueReference, bool AllowSceneObjects = true) where T : UnityEngine.Object {
            GUI.enabled = false;
            ValueReference = EditorGUILayout.ObjectField(Label, ValueReference, typeof(T), AllowSceneObjects) as T;
            GUI.enabled = true;
        }

        /// <summary>
        /// Makes a field using <see cref="EditorGUILayout.LabelField(string, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="Label">The name of the label</param>
        public static void LabelField(string Label) {
            EditorGUILayout.LabelField(Label);
        }

        /// <inheritdoc cref="LabelField(string)"/>
        public static void BoldLabelField(string Label) {
            EditorGUILayout.LabelField(Label, EditorStyles.boldLabel);
        }

        /// <summary>
        /// Makes a vertical group. Code inside the group is passed through an <see cref="Action"/>.
        /// </summary>
        /// <param name="VerticalGroupCode">The code of the layout inside the group.</param>
        public static void VerticalGroup(Action VerticalGroupCode) {
            EditorGUILayout.BeginVertical();
            VerticalGroupCode.Invoke();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Makes an horizontal group. Code inside the group is passed through an <see cref="Action"/>.
        /// </summary>
        /// <param name="HorizontalGroupCode">The code of the layout inside the group.</param>
        public static void HorizontalGroup(Action HorizontalGroupCode) {
            EditorGUILayout.BeginHorizontal();
            HorizontalGroupCode.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Makes a scroll group. Code inside the group is passed through an <see cref="Action"/>
        /// </summary>
        /// <param name="ScrollPosition">The reference of the position of the ScrollGroup</param>
        /// <param name="ScrollGroupCode">The code of the layout inside the group.</param>
        public static void ScrollGroup(ref Vector2 ScrollPosition, Action ScrollGroupCode) {
            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
            ScrollGroupCode.Invoke();
            EditorGUILayout.EndScrollView();
        }

        /// <inheritdoc cref="GUILayout.Button(string, GUILayoutOption[])"/>
        public static bool Button(string Label) {
            return GUILayout.Button(Label);
        }

        /// <inheritdoc cref="GUILayout.Button(string, GUILayoutOption[])"/>
        public static void Button(string Label, Action Code) {
            if (GUILayout.Button(Label)) {
                Code.Invoke();
            }
        }

        /// <inheritdoc cref="GUILayout.Button(string, GUILayoutOption[])"/>
        public static void Button<T1>(string label, Action<T1> code, T1 parameter) {
            if (GUILayout.Button(label)) {
                code.Invoke(parameter);
            }
        }

        /// <inheritdoc cref="GUILayout.Button(string, GUILayoutOption[])"/>
        public static void Button<T1, T2>(string label, Action<T1, T2> code, T1 parameter1, T2 parameter2) {
            if (GUILayout.Button(label)) {
                code.Invoke(parameter1, parameter2);
            }
        }

        /// <inheritdoc cref="GUILayout.Button(string, GUILayoutOption[])"/>
        public static void Button<T1, T2, T3>(string label, Action<T1, T2, T3> code, T1 parameter1, T2 parameter2, T3 parameter3) {
            if (GUILayout.Button(label)) {
                code.Invoke(parameter1, parameter2, parameter3);
            }
        }

        /// <summary>
        /// Makes a small space between fields
        /// </summary>
        /// <param name="quantity">How many spaces to insert.</param>
        public static void Space(uint quantity = 1) {
            for (int i=0; i<quantity; i++)
                EditorGUILayout.Space();
        }
    }
}