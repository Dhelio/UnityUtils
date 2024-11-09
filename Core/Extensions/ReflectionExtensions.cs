using System.Reflection;
using UnityEngine;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extension methods for Reflection operations
    /// </summary>
    public static class ReflectionExtensions {

        /// <summary>
        /// Retrieves the value of the passed field by reflection.
        /// </summary>
        /// <typeparam name="T">Type to retrieve</typeparam>
        /// <param name="target">Target to perform the reflection on</param>
        /// <param name="fieldName">Name of the field to perform the reflection on</param>
        /// <returns>The value</returns>
        public static T GetFieldValue<T>(this MonoBehaviour target, string fieldName) {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = target.GetType().GetField(fieldName, bindingFlags);
            return (T)field.GetValue(target);
        }

        /// <summary>
        /// Sets the value of the passed field by reflection.
        /// </summary>
        /// <typeparam name="T">Type to set</typeparam>
        /// <param name="target">Target to perform the reflection on</param>
        /// <param name="FieldName">Name of the field to set the value to</param>
        /// <param name="Value">The value to set</param>
        public static void SetFieldValue<T>(this MonoBehaviour target, string FieldName, T Value) {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = target.GetType().GetField(FieldName, bindingFlags);
            field.SetValue(target, Value);
        }

        /// <summary>
        /// Retrieves the value of the passed field by reflection.
        /// </summary>
        /// <typeparam name="T">Type to retrieve</typeparam>
        /// <param name="target">Target to perform the reflection on</param>
        /// <param name="FieldName">Name of the field to perform the reflection on</param>
        /// <returns>The value</returns>
        public static T GetFieldValue<T>(this ScriptableObject target, string FieldName) {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = target.GetType().GetField(FieldName, bindingFlags);
            return (T)field.GetValue(target);
        }

        /// <summary>
        /// Sets the value of the passed field by reflection.
        /// </summary>
        /// <typeparam name="T">Type to set</typeparam>
        /// <param name="target">Target to perform the reflection on</param>
        /// <param name="FieldName">Name of the field to set the value to</param>
        /// <param name="Value">The value to set</param>
        public static void SetFieldValue<T>(this ScriptableObject target, string FieldName, T Value) {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = target.GetType().GetField(FieldName, bindingFlags);
            field.SetValue(target, Value);
        }
    }
}