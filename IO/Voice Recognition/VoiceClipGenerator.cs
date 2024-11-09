using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Core.Utilities;
using Castrimaris.IO.Contracts;
using UnityEngine;

/// <summary>
/// Utility script to generate voice lines to use as <see cref="AudioClip"/> later in Unity.
/// </summary>
public class VoiceClipGenerator : MonoBehaviour {

    [Header("Parameters")]
    public bool shouldRunOnStart = false;
    public string[] phrases;

    private async void Start() {
        if (!shouldRunOnStart)
            return;

        int index = 0;
        var synths = GetComponents<ITextToSpeech>();
        foreach (var synth in synths) {
            foreach (var phrase in phrases) {
                Log.D($"Generating audio {index+1}...");
                var clip = await synth.Generate(phrase);
                Log.D($"Done. Exporting audio {index + 1}...");
                AudioClipUtilities.ExportWavToDisk($"{Application.persistentDataPath}/GeneratedSpeech_{index}.wav", clip); //Default path is C:\Users\<username>\AppData\LocalLow\<company name>\<app name>\GeneratedSpeech_<index>.wav
                Log.D($"Done. Audio exported to {Application.persistentDataPath}/GeneratedSpeech_{index}.wav");
                index++;
            }
        }
    }
} 