using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour{

    public static UIManager instance;
    public TextMeshProUGUI ammoText;
    public GameObject deathPanel;
    public TextMeshProUGUI deathMessageText;
    public TextMeshProUGUI deathTimerText;
    public Image healthImage;
    public Color goodHealthColor;
    public Color badHealthColor;
    public TextMeshProUGUI playerNameText;
    public GameObject joinGamePanel;

    private void Awake() {
        instance = this;
    }
    private void Start() {
        ShowJoinGamePanel();
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

    public void ShowJoinGamePanel() {
        joinGamePanel.SetActive(true);
    }
    public void HideJoinGamePanel() {
        joinGamePanel.SetActive(false);
    }

    public void UpdateHealth(int current, int total) {
        float percentage = (float)current / (float)total;
        if (percentage < .3f) {
            healthImage.color = badHealthColor;
        }
        else {
            healthImage.color = goodHealthColor;
        }
        healthImage.fillAmount = percentage;
    }
    public void UpdatePlayerName(string name) {
        playerNameText.text = name;
    }

}
