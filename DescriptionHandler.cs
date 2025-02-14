using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescriptionHandler : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField, Range(1f, 5f)] private float timeToWait = 2.5f;

    [Header("References")]
    [SerializeField] private GameObject descriptionPage = null;
    [SerializeField] private GameObject addToCartPage = null;


    private void Awake() {
        descriptionPage.SetActive(true);
        addToCartPage.SetActive(false);
    }

    public void ShowTimedAddToCartPage() {
        StartCoroutine(ShowAddToCartPageTemporary());
    }

    private IEnumerator ShowAddToCartPageTemporary() {
        var wfs = new WaitForSeconds(timeToWait);

        descriptionPage.SetActive(false);
        addToCartPage.SetActive(true);
        yield return wfs;
        descriptionPage.SetActive(true);
        addToCartPage.SetActive(false);
    }
}
