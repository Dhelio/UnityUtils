using Castrimaris.UI.Contracts;
using TMPro;
using UnityEngine;

namespace Castrimaris.UI {

    /// <summary>
    /// Interface implementation of <see cref="IText"/> for <see cref="TextMeshProUGUI"/> components. Needed this way because it's implemented in a library as a partial class.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshProUGUIInterface : MonoBehaviour, IText {

        private TextMeshProUGUI textMeshPro;

        public string Text { get => textMeshPro.text; set => textMeshPro.text = value; }

        public string GetText() => textMeshPro.text;            

        public void SetText(string text) => textMeshPro.text = text;

        private void Awake() {
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }
    }

}