using Castrimaris.Attributes;
using Castrimaris.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Castrimaris.Accessibility {

    [RequireComponent(typeof(Camera))]
    public class AccessibilityManager : SingletonMonoBehaviour<AccessibilityManager> {

        [Header("References")]
        [ReadOnly, SerializeField] private TextMeshProUGUI subtitles;

        public void EnqueueSubtitles(string text) { }
    }
}
