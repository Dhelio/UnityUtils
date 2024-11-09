using TMPro;
using UnityEngine;

namespace Castrimaris.Core.Monitoring {

    /// <summary>
    /// Intercepts all logs and shows them in the 3D space
    /// </summary>
    public class LogInterceptor3D : MonoBehaviour {

        [Header("References")]
        [SerializeField] private TextMeshPro console;

        public void Append(string message, string stackTrace, LogType type) {
            console.text += "\n";
            console.text += message;
        }

        private void Awake() {
            if (console == null) {
                console = GetComponent<TextMeshPro>();
            }
        }

        private void OnEnable() {
            Application.logMessageReceived += Append;
        }

        private void OnDisable() {
            Application.logMessageReceived -= Append;
        }
    }

}