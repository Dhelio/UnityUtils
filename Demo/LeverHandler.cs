using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LeverHandler : MonoBehaviour {

    [Header("Parameters")]
    [SerializeField] private Color inactiveLedColor = Color.red;
    [SerializeField] private Color activeLedColor = Color.green;

    [Header("References")]
    [SerializeField] private TMPro.TextMeshPro text;
    [SerializeField] private MeshRenderer[] leds;

    public void UpdateTextValue(float NormalizedValue)  => text.text = System.MathF.Round(NormalizedValue,2).ToString();

    public void UpdateLeds(float NormalizedValue) {
        int ledsToActivate = (int)(leds.Length * NormalizedValue);
        for (int i = 0; i < leds.Length; i++) {
            leds[i].material.color = (i <= ledsToActivate) ? activeLedColor : inactiveLedColor;
        }
    }

    private void Awake() {
        foreach (var led in leds)
            led.material.color = inactiveLedColor;
    }

}

