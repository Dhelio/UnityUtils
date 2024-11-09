using TMPro;
using UnityEngine;

namespace Castrimaris.Addressables {

    /// <summary>
    /// HACK: this is a required class for <see cref="TextMeshProUGUI"/> components loaded from Addressables.
    /// It just loads the corresponding <see cref="TMP_FontAsset"/> from Resources.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    [DisallowMultipleComponent]
    public class AddressableGUIFontInitializer : MonoBehaviour {

        private void Awake() {
            var text = GetComponent<TextMeshProUGUI>();
            var fontAsset = Resources.Load<TMP_FontAsset>($"{TMP_Settings.defaultFontAssetPath}{text.font.name}");
            text.font = fontAsset;
            text.UpdateFontAsset();
        }
    }

}