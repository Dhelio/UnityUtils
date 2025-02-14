using Castrimaris.Core;

namespace Castrimaris.UI.Contracts {
    /// <summary>
    /// Contract for objects that have text, such as TextMeshPro's text or Unity UI default Text.
    /// </summary>
    public interface IText : IGameObjectSource {
        public string Text { get; set; }
        public void SetText(string text);
        public string GetText();
    }
}