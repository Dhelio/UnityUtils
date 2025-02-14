using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Castrimaris.Core.Extensions;
using System.Reflection;
using System;

namespace Castrimaris.Core.Editor.Windows {

    [Obsolete("Doesn't work")]
    public class PersistentEventCopyWindow : EditorWindow {

        private MonoBehaviour source;
        private string sourceEventName;
        private MonoBehaviour target;
        private string targetEventName;

        private void OnGUI() {
            Layout.BoldLabelField("boh");
            Layout.ObjectField("Source", ref source);
            Layout.TextField("Source Event Name", ref sourceEventName);
            Layout.ObjectField("Target", ref target);
            Layout.TextField("Target Event Name", ref targetEventName);
            Layout.Button("Copy", Copy);
        }

        private void Copy() {
            var sourceEvent = source.GetFieldValue<UnityEvent>(sourceEventName);
            var targetEvent = target.GetFieldValue<UnityEvent>(targetEventName);

            targetEvent.RemoveAllListeners();

            var sourceCallsField = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic);
            var sourceCalls = sourceCallsField?.GetValue(sourceEvent);

            var targetCallsField = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic);
            targetCallsField?.SetValue(targetEvent, sourceCalls);
        }

        [MenuItem("Tools/Castrimaris/Windows/Persistent Event Copier")]
        private static void OpenWindow() {
            GetWindow<PersistentEventCopyWindow>("Persistent Event Copier");
        }
    }

}