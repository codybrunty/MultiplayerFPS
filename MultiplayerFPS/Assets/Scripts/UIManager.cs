using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour{

    public static UIManager instance;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI gunNameText;
    public GameObject deathPanel;
    public TextMeshProUGUI deathMessageText;
    public TextMeshProUGUI deathTimerText;

    private void Awake() {
        instance = this;
    }

    public void ShowDeathPanel(string damager,float duration) {
        deathMessageText.text = "You were killed by " + damager;
        deathPanel.SetActive(true);
        StartCoroutine(StartDeathTimer(duration));
    }

    IEnumerator StartDeathTimer(float duration) {
        for (float t = 0; t < duration; t+=Time.deltaTime) {
            deathTimerText.text = ((int)(Mathf.Lerp(duration,0,t/duration))).ToString();
            yield return null;
        }
        deathTimerText.text = 0.ToString();
        HideDeathPanel();
    }

    private void HideDeathPanel() {
        deathPanel.SetActive(false);
    }

}
