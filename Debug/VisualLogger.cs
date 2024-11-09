using Castrimaris.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Castrimaris.Monitoring {

    public class VisualLogger : SingletonMonoBehaviour<VisualLogger> {

        private uint currentLineCount = 0;

        [Header("Parameters")]
        public bool ShouldLogToConsole = true;
        public uint MaxLineCount = 30;

        [Header("References")]
        public TextMeshPro LogTMPro = null;

        private void UpdateLog(string NewLine) {
            if (currentLineCount >= MaxLineCount) {
                string text = LogTMPro.text;
                List<string> lines = text.Split(System.Environment.NewLine).ToList();
                lines.RemoveAt(0);
                lines.Add(NewLine);
                LogTMPro.text = lines.ToString();
            } else {
                LogTMPro.text += $"{System.Environment.NewLine}{NewLine}";
            }
        }

        public void LogInfo(string Content) {
            UnityEngine.Debug.Log(Content);
            DateTime time = DateTime.Now;
            Content = $"<color=white>[{time.Hour}:{time.Minute}:{time.Second}] {Content}</color>";
            UpdateLog(Content);
        }

        public void LogWarning(string Content) {
            UnityEngine.Debug.LogWarning(Content);
            DateTime time = DateTime.Now;
            Content = $"<color=yellow>[{time.Hour}:{time.Minute}:{time.Second}] {Content}</color>";
            UpdateLog(Content);
        }

        public void LogError(string Content) {
            UnityEngine.Debug.LogError(Content);
            DateTime time = DateTime.Now;
            Content = $"<color=red>[{time.Hour}:{time.Minute}:{time.Second}] {Content}</color>";
            UpdateLog(Content);
        }

        protected override void Awake() {
            base.Awake();

            if (LogTMPro == null) {
                if (!TryGetComponent<TextMeshPro>(out LogTMPro)) {
                    LogTMPro = this.gameObject.AddComponent<TextMeshPro>();
                }
            }
        }
    }

}