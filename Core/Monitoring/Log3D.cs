using System.Linq;
using TMPro;
using UnityEngine;

namespace Castrimaris.Core.Monitoring {

    /// <summary>
    /// Logs messages in the 3D space
    /// </summary>
    public class Log3D : SingletonMonoBehaviour<Log3D> {

        [Header("References")]
        [SerializeField] protected TextMeshPro console;

        public virtual void Append(string Message) {
            console.text += "\n";
            console.text += Message;
        }

        protected override void Awake() {
            base.Awake();
            if (console == null) {
                console = GetComponent<TextMeshPro>();
            }
        }

    }

}