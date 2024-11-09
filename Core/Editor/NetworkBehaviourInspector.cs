using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Castrimaris.Core.Monitoring;
using Unity.Netcode;

namespace Castrimaris.Attributes {

    /// <summary>
    /// General inspector for MonoBehaviour components. Used to display additional data like exposed methods with <see cref="ExposeInInspector"/>
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NetworkBehaviour), true)]
    public class NetworkBehaviourInspector : Editor {

        Dictionary<MethodInfo, List<object>> _methodParamsDictionary = new Dictionary<MethodInfo, List<object>>();

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            Type type = target.GetType();

            //Retrieve all properties marked with the attribute ExposeInInspector
            var exposedProperties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).
                Where(item => item.IsDefined(typeof(ExposeInInspector), true)).ToArray();

            //Draw all Exposed properties
            if (exposedProperties.Length > 0) {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Exposed Properties", EditorStyles.boldLabel);
                foreach (PropertyInfo propertyInfo in exposedProperties) {
                    if (propertyInfo.IsDefined(typeof(ExposeInInspector), true))
                        CastrimarisDrawProperty(propertyInfo);
                }
            }

            //Retrieve all methods marked with the attribute ExposeInInspector
            var exposedMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).
                Where(item => item.IsDefined(typeof(ExposeInInspector), true)).ToArray();

            //Draw all Exposed methods
            if (exposedMethods.Length > 0) {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Exposed Methods", EditorStyles.boldLabel);
                foreach (MethodInfo methodInfo in exposedMethods) {
                    if (methodInfo.IsDefined(typeof(ExposeInInspector), true))
                        CastrimarisDrawMethod(methodInfo);
                }
            }

            EditorUtility.SetDirty(target);
        }

        private void CastrimarisDrawProperty(PropertyInfo propertyInfo) {
            try {
                if (propertyInfo.GetGetMethod(true) == null)
                    return;

                bool hasSetMethod = propertyInfo.GetSetMethod(true) != null;
                if (hasSetMethod == false)
                    GUI.enabled = false;

                object value = TypeDrawer.Draw(propertyInfo.PropertyType, propertyInfo.Name, propertyInfo.GetValue(target, null));

                if (hasSetMethod)
                    propertyInfo.SetValue(target, value, null);

                GUI.enabled = true;
            } catch (Exception ex) {
                EditorGUILayout.LabelField(ex.ToString());
            }
        }

        /// <summary>
        /// Draws a method in the inspector of the current selected gameobject with corresponding variables
        /// </summary>
        /// <param name="methodInfo"></param>
        private void CastrimarisDrawMethod(MethodInfo methodInfo) {
            try {
                var impossibleParams = methodInfo.GetParameters().Where(item =>
                item.ParameterType != typeof(int) &&
                item.ParameterType != typeof(long) &&
                item.ParameterType != typeof(float) &&
                item.ParameterType != typeof(string) &&
                item.ParameterType != typeof(bool) &&
                item.ParameterType != typeof(Vector2) &&
                item.ParameterType != typeof(Vector3) &&
                item.ParameterType != typeof(Vector4) &&
                item.ParameterType.IsEnum == false &&
                typeof(UnityEngine.Object).IsAssignableFrom(item.ParameterType) == false).ToArray();

                if (impossibleParams.Length > 0) {
                    Log.E($"Couldn't draw method {methodInfo.Name} because one or more of its parameters is not serializable!");
                    return;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(methodInfo.Name);

                List<object> methodParams = null;
                if (_methodParamsDictionary.TryGetValue(methodInfo, out methodParams) == false) {
                    methodParams = new List<object>();
                    _methodParamsDictionary.Add(methodInfo, methodParams);
                }

                EditorGUILayout.BeginVertical();

                ParameterInfo[] parameters = methodInfo.GetParameters();
                for (int i = 0; i < parameters.Length; ++i) {
                    if (methodParams.Count <= i) {
                        if (parameters[i].ParameterType.IsValueType)
                            methodParams.Add(Activator.CreateInstance(parameters[i].ParameterType));
                        else
                            methodParams.Add(null);
                    }

                    methodParams[i] = TypeDrawer.Draw(parameters[i].ParameterType, parameters[i].Name, methodParams[i]);
                }

                if (GUILayout.Button("Invoke")) {
                    object returnValue = methodInfo.Invoke(target, methodParams.ToArray());
                    if (returnValue != null)
                        Debug.Log(returnValue);
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            } catch (Exception ex) {
                EditorGUILayout.LabelField(ex.ToString());
            }
        }
    }
}